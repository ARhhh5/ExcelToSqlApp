using ExcelToSqlApp.Models;
using ExcelToSqlApp.Services;
using Microsoft.Data.SqlClient;

namespace ExcelToSqlApp.Tests;

/// <summary>
/// Runs against a real SQL Server when TEST_SQL_CONNECTION is set (the CI workflow starts
/// one in a container); skipped otherwise.
/// </summary>
public class SqlImportTests : IAsyncLifetime
{
    private static readonly string? MasterConnection = Environment.GetEnvironmentVariable("TEST_SQL_CONNECTION");
    private static readonly TableInfo UnitTable = new("dbo", "SMUNITMS");
    private static readonly TableInfo IdentityTable = new("dbo", "UNIT_IDENTITY");

    private string _database = string.Empty;
    private string _connectionString = string.Empty;

    public async ValueTask InitializeAsync()
    {
        if (MasterConnection is null)
        {
            return;
        }

        _database = $"ExcelToSqlTest_{Guid.NewGuid():N}";
        await ExecuteAsync(MasterConnection, $"CREATE DATABASE [{_database}]");
        _connectionString = new SqlConnectionStringBuilder(MasterConnection) { InitialCatalog = _database }.ConnectionString;

        // Mirrors dbo.SMUNITMS from schema_all_tables.sql (numeric ID without IDENTITY).
        await ExecuteAsync(_connectionString, """
            CREATE TABLE dbo.SMUNITMS ([ID] numeric(18,0) NOT NULL PRIMARY KEY, [Code] varchar(15) NOT NULL,
                [Name] nvarchar(100) NULL, [Descr] nvarchar(400) NULL, [F_Active] char(10) NULL,
                [Wide] decimal(18,6) NULL, [Length] decimal(18,6) NULL, [Hight] decimal(18,6) NULL,
                [Weight] decimal(18,6) NULL, [f_qty_type] varchar(1) NULL);
            CREATE TABLE dbo.UNIT_IDENTITY ([ID] int IDENTITY(1,1) PRIMARY KEY, [Code] varchar(15) NOT NULL UNIQUE,
                [Name] nvarchar(100) NULL, [Created] datetime NOT NULL DEFAULT GETDATE());
            """);
    }

    public async ValueTask DisposeAsync()
    {
        if (MasterConnection is null)
        {
            return;
        }

        SqlConnection.ClearAllPools();
        await ExecuteAsync(MasterConnection, $"ALTER DATABASE [{_database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_database}]");
    }

    [Fact]
    public async Task Import_InsertsThenUpdatesExistingCodes()
    {
        Assert.SkipWhen(MasterConnection is null, "TEST_SQL_CONNECTION not set");

        var first = await ImportAsync(UnitTable, DuplicateMode.Update, TestWorkbooks.SampleUnits);
        Assert.Equal(new ImportResult(6, 0, 0), first);

        var second = await ImportAsync(UnitTable, DuplicateMode.Update,
            [1, "U001", "แผ่นใหม่", "อัปเดต"],
            [2, "U999", "ใหม่", null]);
        Assert.Equal(new ImportResult(1, 1, 0), second);

        var rows = await QueryAsync("SELECT ID, Code, Name, Descr FROM dbo.SMUNITMS ORDER BY ID");
        Assert.Equal(7, rows.Count);
        Assert.Equal(Enumerable.Range(1, 7).Select(i => (decimal)i), rows.Select(r => (decimal)r[0]!));
        Assert.Equal(["U001", "แผ่นใหม่", "อัปเดต"], rows[0][1..].Select(v => v as string));
        Assert.Equal("U999", rows[6][1]);
    }

    [Fact]
    public async Task Import_SkipModeKeepsExistingRows()
    {
        Assert.SkipWhen(MasterConnection is null, "TEST_SQL_CONNECTION not set");

        await ImportAsync(UnitTable, DuplicateMode.Update, [1, "U001", "แผ่น", null]);
        var result = await ImportAsync(UnitTable, DuplicateMode.Skip, [1, "U001", "เปลี่ยน", null], [2, "U002", "KG.", null]);

        Assert.Equal(new ImportResult(1, 0, 1), result);
        Assert.Equal("แผ่น", (await QueryAsync("SELECT Name FROM dbo.SMUNITMS WHERE Code = 'U001'"))[0][0]);
    }

    [Fact]
    public async Task Import_AbortModeRollsBackEverything()
    {
        Assert.SkipWhen(MasterConnection is null, "TEST_SQL_CONNECTION not set");

        await ImportAsync(UnitTable, DuplicateMode.Update, [1, "U002", "KG.", null]);

        var ex = await Assert.ThrowsAsync<DuplicateRowException>(() => ImportAsync(UnitTable, DuplicateMode.Abort,
            [1, "U001", "แผ่น", null],
            [2, "U002", "KG.", null]));

        Assert.Equal(3, ex.ExcelRow);
        Assert.Single(await QueryAsync("SELECT Code FROM dbo.SMUNITMS"));
    }

    [Fact]
    public async Task Import_SqlErrorMidwayRollsBackAndReportsExcelRow()
    {
        Assert.SkipWhen(MasterConnection is null, "TEST_SQL_CONNECTION not set");

        await ExecuteAsync(_connectionString, """
            CREATE TRIGGER dbo.trg_fail ON dbo.SMUNITMS AFTER INSERT AS
            IF EXISTS (SELECT 1 FROM inserted WHERE Code = 'BOOM') THROW 50001, 'forced failure', 1;
            """);

        var ex = await Assert.ThrowsAsync<ImportRowException>(() => ImportAsync(UnitTable, DuplicateMode.Update,
            [1, "U001", "แผ่น", null],
            [2, "U002", "KG.", null],
            [3, "BOOM", "fails", null]));

        Assert.Equal(4, ex.ExcelRow);
        Assert.Equal(50001, Assert.IsType<SqlException>(ex.InnerException).Number);
        Assert.Empty(await QueryAsync("SELECT Code FROM dbo.SMUNITMS"));
    }

    [Fact]
    public async Task Import_IdentityTableLetsDatabaseGenerateIds()
    {
        Assert.SkipWhen(MasterConnection is null, "TEST_SQL_CONNECTION not set");

        var schema = await SchemaService.GetColumnsAsync(_connectionString, IdentityTable);
        var id = schema.Single(c => c.Name == "ID");
        Assert.True(id.IsIdentity && id.IsPrimaryKey);
        Assert.True(schema.Single(c => c.Name == "Code").IsUniqueKey);
        Assert.True(schema.Single(c => c.Name == "Created").HasDefault);

        var result = await ImportAsync(IdentityTable, DuplicateMode.Update, TestWorkbooks.SampleUnits);
        var again = await ImportAsync(IdentityTable, DuplicateMode.Update, [1, "U001", "แผ่น 2", null]);

        Assert.Equal(new ImportResult(6, 0, 0), result);
        Assert.Equal(new ImportResult(0, 1, 0), again);
        Assert.Equal(Enumerable.Range(1, 6), (await QueryAsync("SELECT ID FROM dbo.UNIT_IDENTITY ORDER BY ID")).Select(r => (int)r[0]!));
    }

    private async Task<ImportResult> ImportAsync(TableInfo table, DuplicateMode mode, params object?[][] rows)
    {
        var path = TestWorkbooks.Create("หน่วยนับ", TestWorkbooks.UnitHeaders, rows);
        var sheet = ExcelService.Read(path).Tables[0];
        var schema = await SchemaService.GetColumnsAsync(_connectionString, table);
        var mappings = ColumnMapper.AutoMap(schema, ExcelService.GetDataColumns(sheet));
        var plan = ImportService.Prepare(table, mappings, sheet);

        var reported = new List<int>();
        var result = await ImportService.ImportAsync(_connectionString, plan, mode, new InlineProgress(reported.Add));

        Assert.Equal(plan.Rows.Count, reported.LastOrDefault());
        return result;
    }

    private async Task<List<object?[]>> QueryAsync(string sql)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var rows = new List<object?[]>();
        while (await reader.ReadAsync())
        {
            var values = new object?[reader.FieldCount];
            reader.GetValues(values!);
            rows.Add(values.Select(v => v is DBNull ? null : v).ToArray());
        }
        return rows;
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private sealed class InlineProgress(Action<int> report) : IProgress<int>
    {
        public void Report(int value) => report(value);
    }
}
