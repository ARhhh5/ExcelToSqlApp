namespace ExcelToSqlApp.Models;

public sealed record TableInfo(string Schema, string Name)
{
    public string FullName => $"{Schema}.{Name}";

    public override string ToString() => FullName;
}
