using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using ExcelDataReader;

namespace ExcelToSqlApp.Services;

public static partial class ExcelService
{
    /// <summary>Hidden column holding the original Excel row number (for error messages).</summary>
    public const string RowNumberColumn = "__ExcelRow";

    static ExcelService()
    {
        // Required by ExcelDataReader for legacy .xls (code page) files on .NET Core.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [GeneratedRegex(@"^Column\d+$")]
    private static partial Regex AutoHeaderRegex();

    /// <summary>Reads every sheet; the first row of each sheet is treated as the header row.</summary>
    public static DataSet Read(string path)
    {
        // FileShare.ReadWrite lets us read a workbook that is still open in Excel.
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            UseColumnDataType = false,
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true },
        });

        foreach (DataTable table in dataSet.Tables)
        {
            Clean(table);
        }

        return dataSet;
    }

    public static IReadOnlyList<string> GetDataColumns(DataTable table) => table.Columns
        .Cast<DataColumn>()
        .Where(c => c.ColumnName != RowNumberColumn)
        .Select(c => c.ColumnName)
        .ToList();

    public static int GetExcelRowNumber(DataRow row) => (int)row[RowNumberColumn];

    public static bool IsEmpty(object? value) =>
        value is null || value is DBNull || (value is string s && string.IsNullOrWhiteSpace(s));

    private static void Clean(DataTable table)
    {
        // Header row is Excel row 1, so data starts at row 2.
        var rowNumber = table.Columns.Add(RowNumberColumn, typeof(int));
        for (var i = 0; i < table.Rows.Count; i++)
        {
            table.Rows[i][rowNumber] = i + 2;
        }
        rowNumber.SetOrdinal(0);

        var dataColumns = table.Columns.Cast<DataColumn>().Where(c => c != rowNumber).ToList();

        // Drop rows with no data at all (formatted but empty rows at the bottom of a sheet).
        for (var i = table.Rows.Count - 1; i >= 0; i--)
        {
            var row = table.Rows[i];
            if (dataColumns.All(c => IsEmpty(row[c])))
            {
                table.Rows.RemoveAt(i);
            }
        }

        // Drop untitled columns that hold no data.
        foreach (var column in dataColumns)
        {
            var untitled = AutoHeaderRegex().IsMatch(column.ColumnName);
            if (untitled && table.Rows.Cast<DataRow>().All(r => IsEmpty(r[column])))
            {
                table.Columns.Remove(column);
            }
        }

        foreach (DataColumn column in table.Columns)
        {
            var trimmed = column.ColumnName.Trim();
            if (trimmed != column.ColumnName && !table.Columns.Contains(trimmed))
            {
                column.ColumnName = trimmed;
            }
        }

        table.AcceptChanges();
    }
}
