using System.Data;
using ExcelToSqlApp.Models;
using Microsoft.Data.SqlClient;

namespace ExcelToSqlApp.Services;

public enum DuplicateMode
{
    /// <summary>Update the existing row with the Excel values.</summary>
    Update,
    /// <summary>Keep the existing row and skip the Excel row.</summary>
    Skip,
    /// <summary>Stop the import and roll back everything.</summary>
    Abort,
}

public sealed record PreparedRow(int ExcelRow, object[] Values);

public sealed record ImportResult(int Inserted, int Updated, int Skipped);

public sealed record ExistingRow(int ExcelRow, string KeyDisplay);

/// <summary>Validated, converted data ready to be written to SQL Server.</summary>
public sealed class ImportPlan
{
    public required TableInfo Table { get; init; }

    /// <summary>Columns written on INSERT; indexes match <see cref="PreparedRow.Values"/>.</summary>
    public required IReadOnlyList<ColumnMapping> Columns { get; init; }
    public required IReadOnlyList<PreparedRow> Rows { get; init; }

    public IEnumerable<int> KeyIndexes => Enumerable.Range(0, Columns.Count).Where(i => Columns[i].IsKey);
    public bool HasKeys => KeyIndexes.Any();

    public string KeyDisplay(PreparedRow row) => string.Join(", ",
        KeyIndexes.Select(i => $"{Columns[i].Column.Name} = {row.Values[i]}"));
}

public sealed class ImportValidationException(IReadOnlyList<string> errors)
    : Exception(string.Join(Environment.NewLine, errors))
{
    public IReadOnlyList<string> Errors { get; } = errors;
}

public sealed class ImportRowException(int excelRow, string keyDisplay, Exception inner)
    : Exception(inner.Message, inner)
{
    public int ExcelRow { get; } = excelRow;
    public string KeyDisplay { get; } = keyDisplay;
}

public sealed class DuplicateRowException(int excelRow, string keyDisplay)
    : Exception($"พบข้อมูลซ้ำในระบบ (แถว Excel {excelRow}: {keyDisplay})")
{
    public int ExcelRow { get; } = excelRow;
    public string KeyDisplay { get; } = keyDisplay;
}

public static class ImportService
{
    private const int MaxErrors = 15;
    private const int CommandTimeoutSeconds = 120;

    /// <summary>Validates the mapping and converts every Excel row to SQL-ready values.</summary>
    public static ImportPlan Prepare(TableInfo table, IReadOnlyList<ColumnMapping> mappings, DataTable data)
    {
        var errors = new List<string>();

        foreach (var m in mappings.Where(m => m.Column.IsWritable && m.Source == MappingSource.None
                                              && !m.Column.IsNullable && !m.Column.HasDefault))
        {
            errors.Add($"คอลัมน์ [{m.Column.Name}] เป็น NOT NULL กรุณาเลือกคอลัมน์ Excel หรือระบุค่าคงที่");
        }

        var columns = mappings
            .Where(m => m.Source is MappingSource.Excel or MappingSource.Constant or MappingSource.AutoIncrement)
            .ToList();

        if (!columns.Any(m => m.Source == MappingSource.Excel))
        {
            errors.Add("ยังไม่ได้จับคู่คอลัมน์ Excel กับคอลัมน์ในตาราง SQL");
        }

        foreach (var m in mappings.Where(m => m.IsKey && m.Source is not (MappingSource.Excel or MappingSource.Constant)))
        {
            errors.Add($"คอลัมน์คีย์ [{m.Column.Name}] ต้องจับคู่กับคอลัมน์ Excel");
        }

        foreach (var m in columns.Where(m => m.Source == MappingSource.Excel && !data.Columns.Contains(m.ExcelColumn!)))
        {
            errors.Add($"ไม่พบคอลัมน์ \"{m.ExcelColumn}\" ในชีตที่เลือก");
        }

        if (errors.Count > 0)
        {
            throw new ImportValidationException(errors);
        }

        var constants = new object?[columns.Count];
        for (var i = 0; i < columns.Count; i++)
        {
            if (columns[i].Source != MappingSource.Constant)
            {
                continue;
            }
            try
            {
                constants[i] = ValueConverter.Convert(columns[i].ConstantValue, columns[i].Column);
            }
            catch (Exception ex) when (ex is ValueConversionException or FormatException or OverflowException)
            {
                errors.Add($"ค่าคงที่ของ [{columns[i].Column.Name}] ไม่ถูกต้อง: {ex.Message}");
            }
        }

        var rows = new List<PreparedRow>(data.Rows.Count);
        foreach (DataRow dataRow in data.Rows)
        {
            if (errors.Count >= MaxErrors)
            {
                break;
            }

            var excelRow = ExcelService.GetExcelRowNumber(dataRow);
            var values = new object[columns.Count];

            for (var i = 0; i < columns.Count; i++)
            {
                var m = columns[i];
                switch (m.Source)
                {
                    case MappingSource.Constant:
                        values[i] = constants[i] ?? DBNull.Value;
                        continue;
                    case MappingSource.AutoIncrement:
                        values[i] = DBNull.Value; // assigned during import
                        continue;
                }

                try
                {
                    values[i] = ValueConverter.Convert(dataRow[m.ExcelColumn!], m.Column);
                }
                catch (Exception ex) when (ex is ValueConversionException or FormatException or OverflowException)
                {
                    errors.Add($"แถว {excelRow} คอลัมน์ \"{m.ExcelColumn}\": {ex.Message}");
                    continue;
                }

                if (values[i] is DBNull && (m.IsKey || (!m.Column.IsNullable && !m.Column.HasDefault)))
                {
                    errors.Add($"แถว {excelRow} คอลัมน์ \"{m.ExcelColumn}\" ต้องไม่เป็นค่าว่าง");
                }
            }

            rows.Add(new PreparedRow(excelRow, values));
        }

        if (errors.Count > 0)
        {
            throw new ImportValidationException(errors.Take(MaxErrors).ToList());
        }

        if (rows.Count == 0)
        {
            throw new ImportValidationException(["ไม่พบข้อมูลในชีตที่เลือก"]);
        }

        return new ImportPlan { Table = table, Columns = columns, Rows = rows };
    }

    /// <summary>Lists Excel rows whose key already exists in the target table.</summary>
    public static async Task<List<ExistingRow>> FindExistingAsync(string connectionString, ImportPlan plan, CancellationToken cancellationToken = default)
    {
        var existing = new List<ExistingRow>();
        if (!plan.HasKeys)
        {
            return existing;
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateExistsCommand(plan, connection, transaction: null, withLock: false);

        foreach (var row in plan.Rows)
        {
            SetParameterValues(command, plan, row);
            if ((int)(await command.ExecuteScalarAsync(cancellationToken))! > 0)
            {
                existing.Add(new ExistingRow(row.ExcelRow, plan.KeyDisplay(row)));
            }
        }

        return existing;
    }

    /// <summary>
    /// Inserts/updates all rows inside one transaction. Any failure rolls back every change.
    /// </summary>
    public static async Task<ImportResult> ImportAsync(
        string connectionString,
        ImportPlan plan,
        DuplicateMode duplicateMode,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        int inserted = 0, updated = 0, skipped = 0;
        var current = (PreparedRow?)null;

        try
        {
            var nextIds = await GetAutoIncrementSeedsAsync(plan, connection, transaction, cancellationToken);

            await using var insert = CreateInsertCommand(plan, connection, transaction);
            await using var exists = plan.HasKeys ? CreateExistsCommand(plan, connection, transaction, withLock: true) : null;
            await using var update = plan.HasKeys ? CreateUpdateCommand(plan, connection, transaction) : null;

            var reportEvery = Math.Max(1, plan.Rows.Count / 200);

            for (var r = 0; r < plan.Rows.Count; r++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                current = plan.Rows[r];

                var found = false;
                if (exists is not null)
                {
                    SetParameterValues(exists, plan, current);
                    found = (int)(await exists.ExecuteScalarAsync(cancellationToken))! > 0;
                }

                if (!found)
                {
                    foreach (var (index, next) in nextIds)
                    {
                        current.Values[index] = ValueConverter.Convert(next.Value, plan.Columns[index].Column);
                        next.Value++;
                    }
                    SetParameterValues(insert, plan, current);
                    await insert.ExecuteNonQueryAsync(cancellationToken);
                    inserted++;
                }
                else if (duplicateMode == DuplicateMode.Abort)
                {
                    throw new DuplicateRowException(current.ExcelRow, plan.KeyDisplay(current));
                }
                else if (duplicateMode == DuplicateMode.Update && update is not null)
                {
                    SetParameterValues(update, plan, current);
                    await update.ExecuteNonQueryAsync(cancellationToken);
                    updated++;
                }
                else
                {
                    skipped++;
                }

                if ((r + 1) % reportEvery == 0 || r == plan.Rows.Count - 1)
                {
                    progress?.Report(r + 1);
                }
            }

            await transaction.CommitAsync(cancellationToken);
            return new ImportResult(inserted, updated, skipped);
        }
        catch (Exception ex)
        {
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch (Exception)
            {
                // Connection may already be broken; SQL Server rolls back on disconnect.
            }

            if (ex is DuplicateRowException or OperationCanceledException || current is null)
            {
                throw;
            }
            throw new ImportRowException(current.ExcelRow, plan.KeyDisplay(current), ex);
        }
    }

    private sealed class Counter(decimal value)
    {
        public decimal Value { get; set; } = value;
    }

    private static async Task<List<(int Index, Counter Next)>> GetAutoIncrementSeedsAsync(
        ImportPlan plan, SqlConnection connection, SqlTransaction transaction, CancellationToken cancellationToken)
    {
        var seeds = new List<(int, Counter)>();
        for (var i = 0; i < plan.Columns.Count; i++)
        {
            if (plan.Columns[i].Source != MappingSource.AutoIncrement)
            {
                continue;
            }

            var column = SqlName.Quote(plan.Columns[i].Column.Name);
            // UPDLOCK + HOLDLOCK keeps other sessions from taking the same IDs until we commit.
            var sql = $"SELECT ISNULL(MAX({column}), 0) FROM {SqlName.Quote(plan.Table)} WITH (UPDLOCK, HOLDLOCK)";
            await using var command = new SqlCommand(sql, connection, transaction) { CommandTimeout = CommandTimeoutSeconds };
            var max = System.Convert.ToDecimal(await command.ExecuteScalarAsync(cancellationToken));
            seeds.Add((i, new Counter(max + 1)));
        }
        return seeds;
    }

    private static SqlCommand CreateInsertCommand(ImportPlan plan, SqlConnection connection, SqlTransaction transaction)
    {
        var columns = string.Join(", ", plan.Columns.Select(m => SqlName.Quote(m.Column.Name)));
        var values = string.Join(", ", plan.Columns.Select((_, i) => $"@p{i}"));
        var command = new SqlCommand($"INSERT INTO {SqlName.Quote(plan.Table)} ({columns}) VALUES ({values})", connection, transaction);
        AddParameters(command, plan, Enumerable.Range(0, plan.Columns.Count));
        return command;
    }

    private static SqlCommand CreateUpdateCommand(ImportPlan plan, SqlConnection connection, SqlTransaction transaction)
    {
        var setIndexes = Enumerable.Range(0, plan.Columns.Count)
            .Where(i => !plan.Columns[i].IsKey && plan.Columns[i].Source != MappingSource.AutoIncrement)
            .ToList();

        // Nothing to change besides the key itself: a no-op update keeps the logic uniform.
        var set = setIndexes.Count == 0
            ? $"{SqlName.Quote(plan.Columns[plan.KeyIndexes.First()].Column.Name)} = {SqlName.Quote(plan.Columns[plan.KeyIndexes.First()].Column.Name)}"
            : string.Join(", ", setIndexes.Select(i => $"{SqlName.Quote(plan.Columns[i].Column.Name)} = @p{i}"));

        var command = new SqlCommand($"UPDATE {SqlName.Quote(plan.Table)} SET {set} WHERE {KeyPredicate(plan)}", connection, transaction);
        AddParameters(command, plan, setIndexes.Concat(plan.KeyIndexes));
        return command;
    }

    private static SqlCommand CreateExistsCommand(ImportPlan plan, SqlConnection connection, SqlTransaction? transaction, bool withLock)
    {
        var hint = withLock ? " WITH (UPDLOCK, HOLDLOCK)" : string.Empty;
        var command = new SqlCommand($"SELECT COUNT(1) FROM {SqlName.Quote(plan.Table)}{hint} WHERE {KeyPredicate(plan)}", connection, transaction);
        AddParameters(command, plan, plan.KeyIndexes);
        return command;
    }

    private static string KeyPredicate(ImportPlan plan) => string.Join(" AND ",
        plan.KeyIndexes.Select(i => $"{SqlName.Quote(plan.Columns[i].Column.Name)} = @p{i}"));

    private static void AddParameters(SqlCommand command, ImportPlan plan, IEnumerable<int> indexes)
    {
        command.CommandTimeout = CommandTimeoutSeconds;
        foreach (var i in indexes)
        {
            var column = plan.Columns[i].Column;
            var parameter = new SqlParameter($"@p{i}", column.SqlDbType);
            if (column.IsString)
            {
                parameter.Size = column.MaxChars > 0 ? column.MaxChars : -1;
            }
            else if (column.SqlDbType == SqlDbType.Decimal)
            {
                parameter.Precision = column.Precision;
                parameter.Scale = column.Scale;
            }
            command.Parameters.Add(parameter);
        }
    }

    private static void SetParameterValues(SqlCommand command, ImportPlan plan, PreparedRow row)
    {
        foreach (SqlParameter parameter in command.Parameters)
        {
            parameter.Value = row.Values[int.Parse(parameter.ParameterName.AsSpan(2))];
        }
    }
}
