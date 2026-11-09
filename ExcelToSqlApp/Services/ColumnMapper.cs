using ExcelToSqlApp.Models;

namespace ExcelToSqlApp.Services;

/// <summary>Matches Excel headers (Thai or English) to SQL columns and suggests a target table.</summary>
public static class ColumnMapper
{
    /// <summary>Running-number columns in Excel that should never be imported.</summary>
    private static readonly HashSet<string> SkippedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "no", "#", "ลำดับ", "ลำดับที่", "seq", "rowno",
    };

    /// <summary>Header keyword → candidate SQL column names, in priority order. Longer keywords win.</summary>
    private static readonly (string Keyword, string[] Columns)[] Synonyms =
    [
        ("ชื่อภาษาอังกฤษ", ["Name_En", "Name_Eng", "NameEn"]),
        ("ชื่ออังกฤษ", ["Name_En", "Name_Eng", "NameEn"]),
        ("ชื่อภาษาไทย", ["Name_Th", "Name_Thai", "Name"]),
        ("ชื่อไทย", ["Name_Th", "Name_Thai", "Name"]),
        ("englishname", ["Name_En", "Name_Eng"]),
        ("nameen", ["Name_En", "Name_Eng"]),
        ("รายละเอียด", ["Descr", "Description", "Desc", "Detail"]),
        ("คำอธิบาย", ["Descr", "Description", "Desc"]),
        ("description", ["Descr", "Description", "Desc"]),
        ("หมายเหตุ", ["Remark", "Remark1", "Remarks", "Note"]),
        ("remark", ["Remark", "Remark1", "Remarks"]),
        ("น้ำหนัก", ["Weight"]),
        ("กว้าง", ["Wide", "Width"]),
        ("ยาว", ["Length"]),
        ("สูง", ["Hight", "Height"]),
        ("สถานะ", ["F_Active", "Status", "Active"]),
        ("active", ["F_Active", "Active", "Status"]),
        ("descr", ["Descr", "Description"]),
        ("รหัส", ["Code"]),
        ("code", ["Code"]),
        ("ชื่อ", ["Name", "Name_Th", "Name_Thai"]),
        ("name", ["Name", "Name_Th"]),
    ];

    /// <summary>Thai keyword in sheet/file names → fragment of ERP master table names.</summary>
    private static readonly (string Keyword, string TableFragment)[] TableHints =
    [
        ("หน่วยนับ", "UNIT"),
        ("หน่วย", "UNIT"),
        ("unit", "UNIT"),
        ("สินค้า", "ITEM"),
        ("ลูกค้า", "CUST"),
        ("ผู้จำหน่าย", "VEND"),
        ("ผู้ขาย", "VEND"),
        ("แผนก", "DEPT"),
        ("พนักงาน", "EMPY"),
        ("คลัง", "WHSE"),
    ];

    public static string Normalize(string text) => new(text
        .Trim()
        .ToLowerInvariant()
        .Where(ch => !char.IsWhiteSpace(ch) && ch is not ('.' or '_' or '-' or ':' or '(' or ')'))
        .ToArray());

    public static bool IsSkippedHeader(string header) => SkippedHeaders.Contains(Normalize(header));

    public static List<ColumnMapping> AutoMap(IReadOnlyList<ColumnSchema> columns, IReadOnlyList<string> excelHeaders)
    {
        var mappings = columns.Select(c => new ColumnMapping
        {
            Column = c,
            AutoIncrement = c.CanAutoGenerate,
        }).ToList();

        var writable = mappings.Where(m => m.Column.IsWritable).ToList();
        var used = new HashSet<ColumnMapping>();

        ColumnMapping? Find(string name) => writable.FirstOrDefault(m =>
            !used.Contains(m) && Normalize(m.Column.Name) == Normalize(name));

        var pending = excelHeaders.Where(h => !IsSkippedHeader(h)).ToList();

        // Pass 1: exact column-name matches (e.g. header "Code" → [Code]).
        foreach (var header in pending.ToList())
        {
            if (Find(header) is { } match)
            {
                match.ExcelColumn = header;
                used.Add(match);
                pending.Remove(header);
            }
        }

        // Pass 2: keyword synonyms (e.g. "รหัสหน่วยนับ" → [Code], "ชื่อหน่วยนับ" → [Name]).
        foreach (var header in pending)
        {
            var normalized = Normalize(header);
            foreach (var (keyword, candidates) in Synonyms)
            {
                if (!normalized.Contains(keyword))
                {
                    continue;
                }

                var match = candidates.Select(Find).FirstOrDefault(m => m is not null);
                if (match is not null)
                {
                    match.ExcelColumn = header;
                    used.Add(match);
                    break;
                }
            }
        }

        foreach (var mapping in mappings.Where(m => m.Source == MappingSource.Excel))
        {
            // Mapped values replace any auto-generated value.
            mapping.AutoIncrement = false;
        }

        SuggestKeys(mappings);
        return mappings;
    }

    /// <summary>
    /// Picks the columns used to detect existing rows: mapped primary key columns first,
    /// then mapped unique columns, then a mapped "Code" column.
    /// </summary>
    public static void SuggestKeys(IReadOnlyList<ColumnMapping> mappings)
    {
        foreach (var m in mappings)
        {
            m.IsKey = false;
        }

        bool Supplied(ColumnMapping m) => m.Source is MappingSource.Excel;

        var pk = mappings.Where(m => m.Column.IsPrimaryKey).ToList();
        if (pk.Count > 0 && pk.All(Supplied))
        {
            pk.ForEach(m => m.IsKey = true);
            return;
        }

        var unique = mappings.Where(m => m.Column.IsUniqueKey && Supplied(m)).ToList();
        if (unique.Count > 0)
        {
            unique.ForEach(m => m.IsKey = true);
            return;
        }

        var code = mappings.FirstOrDefault(m => Supplied(m)
            && m.Column.Name.Equals("Code", StringComparison.OrdinalIgnoreCase));
        if (code is not null)
        {
            code.IsKey = true;
        }
    }

    /// <summary>Suggests a target table from the sheet or file name, or returns null.</summary>
    public static TableInfo? SuggestTable(IReadOnlyList<TableInfo> tables, params string[] names)
    {
        foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            var normalized = Normalize(name);
            var exact = tables.FirstOrDefault(t => Normalize(t.Name) == normalized || Normalize(t.FullName) == normalized);
            if (exact is not null)
            {
                return exact;
            }
        }

        foreach (var name in names.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            var normalized = Normalize(name);
            foreach (var (keyword, fragment) in TableHints)
            {
                if (!normalized.Contains(keyword))
                {
                    continue;
                }

                // Prefer master tables (ERP convention: *MS), then the shortest name.
                var candidate = tables
                    .Where(t => t.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(t => t.Name.EndsWith("MS", StringComparison.OrdinalIgnoreCase))
                    .ThenBy(t => t.Name.Length)
                    .ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();
                if (candidate is not null)
                {
                    return candidate;
                }
            }
        }

        return null;
    }
}
