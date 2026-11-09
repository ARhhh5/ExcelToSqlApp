using System.Globalization;
using ExcelToSqlApp.Models;

namespace ExcelToSqlApp.Services;

public sealed class ValueConversionException(string message) : Exception(message);

/// <summary>Converts Excel cell values (string / double / DateTime / bool) to the CLR type of a SQL column.</summary>
public static class ValueConverter
{
    private static readonly CultureInfo[] Cultures = [CultureInfo.InvariantCulture, CultureInfo.CurrentCulture];

    public static object Convert(object? value, ColumnSchema column)
    {
        if (ExcelService.IsEmpty(value))
        {
            return DBNull.Value;
        }

        var type = column.DataType.ToLowerInvariant();
        return type switch
        {
            "char" or "varchar" or "nchar" or "nvarchar" or "text" or "ntext" => ToText(value!, column),
            "tinyint" => checked((byte)ToDecimal(value!, wholeNumber: true)),
            "smallint" => checked((short)ToDecimal(value!, wholeNumber: true)),
            "int" => checked((int)ToDecimal(value!, wholeNumber: true)),
            "bigint" => checked((long)ToDecimal(value!, wholeNumber: true)),
            "decimal" or "numeric" or "money" or "smallmoney" => ToDecimal(value!, wholeNumber: false),
            "float" => ToDouble(value!),
            "real" => (float)ToDouble(value!),
            "bit" => ToBoolean(value!),
            "date" or "datetime" or "datetime2" or "smalldatetime" => ToDateTime(value!),
            "uniqueidentifier" => Guid.Parse(value!.ToString()!.Trim()),
            _ => value!.ToString()!.Trim(),
        };
    }

    private static string ToText(object value, ColumnSchema column)
    {
        var text = value switch
        {
            // ERP tables store dates as varchar(8) yyyyMMdd.
            DateTime dt when column.MaxChars == 8 => dt.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            double d => d.ToString("0.###############", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };

        text = text.Trim();
        if (column.MaxChars > 0 && text.Length > column.MaxChars)
        {
            throw new ValueConversionException($"ข้อความยาว {text.Length} ตัวอักษร เกินขนาดคอลัมน์ ({column.MaxChars})");
        }
        return text;
    }

    private static decimal ToDecimal(object value, bool wholeNumber)
    {
        decimal result;
        switch (value)
        {
            case double d:
                result = (decimal)d;
                break;
            case decimal m:
                result = m;
                break;
            case bool b:
                result = b ? 1 : 0;
                break;
            default:
                var text = value.ToString()!.Trim().Replace(",", string.Empty);
                if (!Cultures.Any(c => decimal.TryParse(text, NumberStyles.Number, c, out result)))
                {
                    throw new ValueConversionException($"\"{value}\" ไม่ใช่ตัวเลข");
                }
                break;
        }

        if (wholeNumber && result != decimal.Truncate(result))
        {
            throw new ValueConversionException($"\"{value}\" ต้องเป็นจำนวนเต็ม");
        }
        return result;
    }

    private static double ToDouble(object value) => value switch
    {
        double d => d,
        _ => (double)ToDecimal(value, wholeNumber: false),
    };

    private static bool ToBoolean(object value)
    {
        if (value is bool b) return b;
        if (value is double d) return d != 0;

        return value.ToString()!.Trim().ToLowerInvariant() switch
        {
            "1" or "true" or "y" or "yes" or "t" or "ใช่" => true,
            "0" or "false" or "n" or "no" or "f" or "ไม่ใช่" => false,
            _ => throw new ValueConversionException($"\"{value}\" ไม่ใช่ค่า True/False"),
        };
    }

    private static DateTime ToDateTime(object value)
    {
        if (value is DateTime dt) return dt;
        if (value is double oa) return DateTime.FromOADate(oa);

        var text = value.ToString()!.Trim();
        if (DateTime.TryParseExact(text, ["yyyyMMdd", "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy"],
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
        {
            return exact;
        }
        if (Cultures.Any(c => DateTime.TryParse(text, c, DateTimeStyles.None, out exact)))
        {
            return exact;
        }
        throw new ValueConversionException($"\"{value}\" ไม่ใช่วันที่");
    }
}
