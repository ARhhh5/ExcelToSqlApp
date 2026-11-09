using System.Text.Json;
using ExcelToSqlApp.Models;

namespace ExcelToSqlApp.Services;

public static class ClientRepository
{
    public const string FileName = "clients.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static string FilePath => Path.Combine(AppContext.BaseDirectory, FileName);

    public static List<ClientProfile> Load()
    {
        if (!File.Exists(FilePath))
        {
            throw new FileNotFoundException($"ไม่พบไฟล์ {FileName}", FilePath);
        }

        var json = File.ReadAllText(FilePath);
        var clients = JsonSerializer.Deserialize<List<ClientProfile>>(json, JsonOptions) ?? [];

        return clients
            .Where(c => !string.IsNullOrWhiteSpace(c.ClientName))
            .OrderBy(c => c.ClientName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
