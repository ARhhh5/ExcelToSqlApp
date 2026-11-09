using System.Data;

namespace ExcelToSqlApp.Models;

/// <summary>Column metadata read from sys.columns for the selected target table.</summary>
public sealed class ColumnSchema
{
    private static readonly HashSet<string> IntegerTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "tinyint", "smallint", "int", "bigint",
    };

    private static readonly HashSet<string> UnicodeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "nchar", "nvarchar", "ntext",
    };

    public required string Name { get; init; }
    public required string DataType { get; init; }

    /// <summary>Raw sys.columns.max_length in bytes (-1 = MAX).</summary>
    public short MaxLengthBytes { get; init; }
    public byte Precision { get; init; }
    public byte Scale { get; init; }
    public bool IsNullable { get; init; }
    public bool IsIdentity { get; init; }
    public bool IsComputed { get; init; }
    public bool HasDefault { get; init; }
    public bool IsPrimaryKey { get; init; }
    public bool IsUniqueKey { get; init; }

    public bool IsString => DataType.Contains("char", StringComparison.OrdinalIgnoreCase)
        || DataType.Equals("text", StringComparison.OrdinalIgnoreCase)
        || DataType.Equals("ntext", StringComparison.OrdinalIgnoreCase);

    public bool IsWholeNumber => IntegerTypes.Contains(DataType)
        || ((DataType.Equals("numeric", StringComparison.OrdinalIgnoreCase)
             || DataType.Equals("decimal", StringComparison.OrdinalIgnoreCase)) && Scale == 0);

    /// <summary>Maximum characters for string columns, or -1 for MAX / non-string.</summary>
    public int MaxChars
    {
        get
        {
            if (!IsString || MaxLengthBytes <= 0)
            {
                return -1;
            }
            return UnicodeTypes.Contains(DataType) ? MaxLengthBytes / 2 : MaxLengthBytes;
        }
    }

    /// <summary>Whether the import may write a value into this column.</summary>
    public bool IsWritable => !IsIdentity && !IsComputed
        && !DataType.Equals("timestamp", StringComparison.OrdinalIgnoreCase)
        && !DataType.Equals("rowversion", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// A non-identity numeric ID (common in ERP tables) that must be generated as MAX+1
    /// when the Excel file does not supply it.
    /// </summary>
    public bool CanAutoGenerate => IsWritable && IsWholeNumber && !IsNullable && !HasDefault
        && (IsPrimaryKey || Name.Equals("ID", StringComparison.OrdinalIgnoreCase));

    public string TypeDisplay => DataType.ToLowerInvariant() switch
    {
        "char" or "varchar" or "nchar" or "nvarchar" or "binary" or "varbinary" =>
            $"{DataType}({(MaxChars < 0 && MaxLengthBytes == -1 ? "max" : (IsString ? MaxChars : MaxLengthBytes).ToString())})",
        "decimal" or "numeric" => $"{DataType}({Precision},{Scale})",
        _ => DataType,
    };

    public string AttributesDisplay
    {
        get
        {
            var parts = new List<string>();
            if (IsPrimaryKey) parts.Add("PK");
            if (IsIdentity) parts.Add("Identity");
            if (IsComputed) parts.Add("Computed");
            if (!IsPrimaryKey && IsUniqueKey) parts.Add("Unique");
            parts.Add(IsNullable ? "NULL" : "NOT NULL");
            if (HasDefault) parts.Add("Default");
            return string.Join(" · ", parts);
        }
    }

    public SqlDbType SqlDbType => DataType.ToLowerInvariant() switch
    {
        "numeric" => SqlDbType.Decimal,
        "rowversion" => SqlDbType.Timestamp,
        "sql_variant" => SqlDbType.Variant,
        _ => Enum.TryParse<SqlDbType>(DataType, ignoreCase: true, out var t) ? t : SqlDbType.NVarChar,
    };
}
