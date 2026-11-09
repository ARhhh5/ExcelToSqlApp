using ExcelToSqlApp.Models;
using Microsoft.Data.SqlClient;

namespace ExcelToSqlApp.Services;

public static class SchemaService
{
    private const string ColumnsQuery = """
        SELECT
            c.name,
            t.name AS type_name,
            c.max_length,
            c.precision,
            c.scale,
            c.is_nullable,
            c.is_identity,
            c.is_computed,
            CAST(CASE WHEN c.default_object_id <> 0 THEN 1 ELSE 0 END AS bit) AS has_default,
            CAST(CASE WHEN EXISTS (
                SELECT 1 FROM sys.index_columns ic
                JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                WHERE ic.object_id = c.object_id AND ic.column_id = c.column_id AND i.is_primary_key = 1
            ) THEN 1 ELSE 0 END AS bit) AS is_pk,
            CAST(CASE WHEN EXISTS (
                SELECT 1 FROM sys.index_columns ic
                JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                WHERE ic.object_id = c.object_id AND ic.column_id = c.column_id
                  AND i.is_unique = 1 AND i.is_primary_key = 0
            ) THEN 1 ELSE 0 END AS bit) AS is_unique
        FROM sys.columns c
        JOIN sys.types t ON t.user_type_id = c.user_type_id
        WHERE c.object_id = OBJECT_ID(@table)
        ORDER BY c.column_id;
        """;

    public static async Task<List<ColumnSchema>> GetColumnsAsync(string connectionString, TableInfo table, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(ColumnsQuery, connection);
        command.Parameters.AddWithValue("@table", SqlName.Quote(table));

        var columns = new List<ColumnSchema>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(new ColumnSchema
            {
                Name = reader.GetString(0),
                DataType = reader.GetString(1),
                MaxLengthBytes = reader.GetInt16(2),
                Precision = reader.GetByte(3),
                Scale = reader.GetByte(4),
                IsNullable = reader.GetBoolean(5),
                IsIdentity = reader.GetBoolean(6),
                IsComputed = reader.GetBoolean(7),
                HasDefault = reader.GetBoolean(8),
                IsPrimaryKey = reader.GetBoolean(9),
                IsUniqueKey = reader.GetBoolean(10),
            });
        }

        if (columns.Count == 0)
        {
            throw new InvalidOperationException($"ไม่พบตาราง {table.FullName} หรือไม่มีสิทธิ์อ่านโครงสร้างตาราง");
        }

        return columns;
    }
}

public static class SqlName
{
    public static string Quote(string identifier) => "[" + identifier.Replace("]", "]]") + "]";

    public static string Quote(TableInfo table) => $"{Quote(table.Schema)}.{Quote(table.Name)}";
}
