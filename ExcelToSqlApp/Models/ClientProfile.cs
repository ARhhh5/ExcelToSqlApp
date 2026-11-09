namespace ExcelToSqlApp.Models;

public sealed class ClientProfile
{
    public string ClientName { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public override string ToString() => ClientName;
}
