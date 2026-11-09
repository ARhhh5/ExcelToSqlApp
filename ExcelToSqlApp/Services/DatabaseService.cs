using ExcelToSqlApp.Models;
using Microsoft.Data.SqlClient;

namespace ExcelToSqlApp.Services;

public static class DatabaseService
{
    private const string TablesQuery = """
        SELECT TABLE_SCHEMA, TABLE_NAME
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_TYPE = 'BASE TABLE'
        ORDER BY TABLE_SCHEMA, TABLE_NAME;
        """;

    public static string BuildConnectionString(string server, string database, string user, string password)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server.Trim(),
            InitialCatalog = database.Trim(),
            UserID = user.Trim(),
            Password = password,
            ConnectTimeout = 10,
            // On-premise ERP servers typically use self-signed certificates, which the
            // SqlClient default (Encrypt=Mandatory) would otherwise reject.
            TrustServerCertificate = true,
            ApplicationName = "ExcelToSqlApp",
        };

        return builder.ConnectionString;
    }

    public static async Task<List<TableInfo>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var tables = new List<TableInfo>();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(TablesQuery, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add(new TableInfo(reader.GetString(0), reader.GetString(1)));
        }

        return tables;
    }

    /// <summary>Translates SqlClient errors into short, user-friendly Thai messages.</summary>
    public static string GetFriendlyMessage(SqlException ex, string server, string database, string user)
    {
        return ex.Number switch
        {
            18456 => $"เข้าสู่ระบบไม่สำเร็จ: ชื่อผู้ใช้ \"{user}\" หรือรหัสผ่านไม่ถูกต้อง",
            18452 => "Server ไม่อนุญาตให้ใช้ SQL Server Authentication กรุณาตรวจสอบการตั้งค่า Server",
            18486 => $"บัญชีผู้ใช้ \"{user}\" ถูกล็อก กรุณาติดต่อผู้ดูแลระบบ",
            18487 or 18488 => $"รหัสผ่านของผู้ใช้ \"{user}\" หมดอายุหรือต้องเปลี่ยนรหัสผ่านก่อนใช้งาน",
            4060 => $"ไม่พบฐานข้อมูล \"{database}\" หรือผู้ใช้ไม่มีสิทธิ์เข้าถึงฐานข้อมูลนี้",
            916 => $"ผู้ใช้ \"{user}\" ไม่มีสิทธิ์เข้าถึงฐานข้อมูล \"{database}\"",
            229 => "ผู้ใช้ไม่มีสิทธิ์อ่านรายชื่อตารางในฐานข้อมูลนี้",
            -2 => "หมดเวลาในการเชื่อมต่อ (Timeout) กรุณาตรวจสอบเครือข่ายหรือ Server",
            -1 or 2 or 53 or 26 or 40 or 1225 or 10060 or 10061 or 11001 or 11004 =>
                $"เชื่อมต่อ Server \"{server}\" ไม่สำเร็จ\n\n" +
                "• ตรวจสอบชื่อ Server / IP / Instance ให้ถูกต้อง\n" +
                "• ตรวจสอบว่า SQL Server เปิดใช้งานและอนุญาต Remote Connection\n" +
                "• ตรวจสอบ Firewall และพอร์ต (ค่าเริ่มต้น 1433)",
            _ => $"เกิดข้อผิดพลาดจาก SQL Server (รหัส {ex.Number})\n\n{ex.Message}",
        };
    }
}
