using ExcelToSqlApp.Models;
using ExcelToSqlApp.Services;

namespace ExcelToSqlApp.Tests;

public class MappingTests
{
    private static ColumnSchema Col(string name, string type, short len = 0, bool nullable = true,
        byte precision = 0, byte scale = 0, bool pk = false, bool identity = false) => new()
    {
        Name = name, DataType = type, MaxLengthBytes = len, IsNullable = nullable,
        Precision = precision, Scale = scale, IsPrimaryKey = pk, IsIdentity = identity,
    };

    // dbo.SMUNITMS as defined in schema_all_tables.sql (ID is numeric, not IDENTITY).
    private static List<ColumnSchema> SmUnitMs() =>
    [
        Col("ID", "numeric", 9, nullable: false, precision: 18),
        Col("Code", "varchar", 15, nullable: false),
        Col("Name", "nvarchar", 200),
        Col("Descr", "nvarchar", 800),
        Col("F_Active", "char", 10),
        Col("Wide", "decimal", 9, precision: 18, scale: 6),
    ];

    [Fact]
    public void Read_SkipsEmptyRowsAndKeepsExcelRowNumbers()
    {
        var path = TestWorkbooks.Create("หน่วยนับ", TestWorkbooks.UnitHeaders, TestWorkbooks.SampleUnits);
        var sheet = ExcelService.Read(path).Tables["หน่วยนับ"]!;

        Assert.Equal(6, sheet.Rows.Count);
        Assert.Equal(TestWorkbooks.UnitHeaders, ExcelService.GetDataColumns(sheet));
        Assert.Equal(2, ExcelService.GetExcelRowNumber(sheet.Rows[0]));
        Assert.Equal(7, ExcelService.GetExcelRowNumber(sheet.Rows[5]));
    }

    [Fact]
    public void AutoMap_MapsThaiHeaders_SkipsNo_AndGeneratesNonIdentityId()
    {
        var mappings = ColumnMapper.AutoMap(SmUnitMs(), TestWorkbooks.UnitHeaders);
        var byName = mappings.ToDictionary(m => m.Column.Name);

        Assert.Equal(MappingSource.AutoIncrement, byName["ID"].Source);
        Assert.Equal("รหัสหน่วยนับ", byName["Code"].ExcelColumn);
        Assert.Equal("ชื่อหน่วยนับ", byName["Name"].ExcelColumn);
        Assert.Equal("รายละเอียด", byName["Descr"].ExcelColumn);
        Assert.DoesNotContain(mappings, m => m.ExcelColumn == "No.");
        Assert.Equal(["Code"], mappings.Where(m => m.IsKey).Select(m => m.Column.Name));
    }

    [Fact]
    public void AutoMap_LeavesIdentityColumnToDatabase()
    {
        var columns = SmUnitMs();
        columns[0] = Col("ID", "int", 4, nullable: false, pk: true, identity: true);

        var id = ColumnMapper.AutoMap(columns, TestWorkbooks.UnitHeaders).Single(m => m.Column.Name == "ID");

        Assert.Equal(MappingSource.Identity, id.Source);
        Assert.False(id.IsKey);
    }

    [Fact]
    public void AutoMap_UsesMappedPrimaryKeyAsDuplicateKey()
    {
        var columns = SmUnitMs();
        columns[1] = Col("Code", "varchar", 15, nullable: false, pk: true);

        var keys = ColumnMapper.AutoMap(columns, TestWorkbooks.UnitHeaders).Where(m => m.IsKey).Select(m => m.Column.Name);

        Assert.Equal(["Code"], keys);
    }

    [Fact]
    public void SuggestTable_FindsUnitMasterFromThaiSheetName()
    {
        TableInfo[] tables = [new("dbo", "SMUNITCV"), new("dbo", "SMUNITMS"), new("dbo", "SMITEMMS")];

        Assert.Equal(new TableInfo("dbo", "SMUNITMS"), ColumnMapper.SuggestTable(tables, "หน่วยนับ", "import"));
        Assert.Equal(new TableInfo("dbo", "SMITEMMS"), ColumnMapper.SuggestTable(tables, "SMITEMMS"));
        Assert.Null(ColumnMapper.SuggestTable(tables, "Sheet1", "data"));
    }

    [Fact]
    public void Prepare_ReportsEmptyAndTooLongValuesWithExcelRowNumbers()
    {
        var path = TestWorkbooks.Create("Sheet1", TestWorkbooks.UnitHeaders,
            [1, "U100", "ok", null],
            [2, null, "missing code", null],
            [3, "THIS_CODE_IS_WAY_TOO_LONG", "too long", null]);
        var sheet = ExcelService.Read(path).Tables[0];
        var mappings = ColumnMapper.AutoMap(SmUnitMs(), ExcelService.GetDataColumns(sheet));

        var ex = Assert.Throws<ImportValidationException>(() =>
            ImportService.Prepare(new TableInfo("dbo", "SMUNITMS"), mappings, sheet));

        Assert.Equal(2, ex.Errors.Count);
        Assert.StartsWith("แถว 3 ", ex.Errors[0]);
        Assert.StartsWith("แถว 4 ", ex.Errors[1]);
    }

    [Fact]
    public void Prepare_RequiresNotNullColumnsToBeMapped()
    {
        var path = TestWorkbooks.Create("Sheet1", ["No.", "ชื่อหน่วยนับ"], [1, "แผ่น"]);
        var sheet = ExcelService.Read(path).Tables[0];
        var mappings = ColumnMapper.AutoMap(SmUnitMs(), ExcelService.GetDataColumns(sheet));

        var ex = Assert.Throws<ImportValidationException>(() =>
            ImportService.Prepare(new TableInfo("dbo", "SMUNITMS"), mappings, sheet));

        Assert.Contains(ex.Errors, e => e.Contains("[Code]"));
    }

    [Theory]
    [InlineData(1.0, "varchar", "1")]
    [InlineData("  U001 ", "varchar", "U001")]
    [InlineData("1,234.50", "decimal", "1234.50")]
    [InlineData("Y", "bit", "True")]
    public void ValueConverter_ConvertsExcelValues(object input, string type, string expected)
    {
        var column = Col("X", type, 50, precision: 18, scale: 2);

        var value = ValueConverter.Convert(input, column);

        Assert.Equal(expected, Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ValueConverter_FormatsDatesAsYyyymmddForVarchar8()
    {
        var column = Col("Doc_date", "varchar", 8);

        Assert.Equal("20261109", ValueConverter.Convert(new DateTime(2026, 11, 9), column));
    }
}
