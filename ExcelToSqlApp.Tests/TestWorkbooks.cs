using ClosedXML.Excel;

namespace ExcelToSqlApp.Tests;

internal static class TestWorkbooks
{
    public static readonly string[] UnitHeaders = ["No.", "รหัสหน่วยนับ", "ชื่อหน่วยนับ", "รายละเอียด"];

    /// <summary>Same layout as the sample unit-of-measure file (No. / code / name / description).</summary>
    public static readonly object?[][] SampleUnits =
    [
        [1, "U001", "แผ่น", null],
        [2, "U002", "KG.", null],
        [3, "U003", "กล่อง", null],
        [4, "U004", "PCS", null],
        [5, "U005", "กรัม", null],
        [6, "U006", "ห่อ", null],
    ];

    public static string Create(string sheetName, string[] headers, params object?[][] rows)
    {
        var path = Path.Combine(Path.GetTempPath(), $"excel-to-sql-{Guid.NewGuid():N}.xlsx");
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(sheetName);

        for (var c = 0; c < headers.Length; c++)
        {
            sheet.Cell(1, c + 1).Value = headers[c];
        }

        for (var r = 0; r < rows.Length; r++)
        {
            for (var c = 0; c < rows[r].Length; c++)
            {
                sheet.Cell(r + 2, c + 1).Value = XLCellValue.FromObject(rows[r][c]);
            }
        }

        // Formatted-but-empty rows below the data, as Excel often leaves behind.
        sheet.Range(rows.Length + 2, 1, rows.Length + 4, headers.Length).Style.Fill.BackgroundColor = XLColor.LightGray;
        workbook.Worksheets.Add("Sheet2");
        workbook.SaveAs(path);
        return path;
    }
}
