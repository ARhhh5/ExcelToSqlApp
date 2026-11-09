using System.Data;
using ExcelDataReader.Exceptions;
using ExcelToSqlApp.Models;
using ExcelToSqlApp.Services;
using Microsoft.Data.SqlClient;

namespace ExcelToSqlApp.Controls;

public partial class ImportView : UserControl
{
    private const string NotImportedItem = "— ไม่นำเข้า —";
    private const string AutoIncrementItem = "⚙ สร้างอัตโนมัติ (MAX + 1)";
    private const string IdentityText = "⚙ Identity (ฐานข้อมูลสร้างให้)";
    private const string ComputedText = "— Computed (ไม่นำเข้า) —";

    private sealed record DuplicateOption(DuplicateMode Mode, string Text)
    {
        public override string ToString() => Text;
    }

    private string? _connectionString;
    private IReadOnlyList<TableInfo> _tables = [];
    private DataSet? _workbook;
    private string? _filePath;
    private TableInfo? _targetTable;
    private List<ColumnSchema>? _schema;
    private List<ColumnMapping> _mappings = [];
    private bool _targetChosenByUser;
    private bool _suppressEvents;
    private int _schemaRequest;
    private bool _busy;

    /// <summary>Raised when an import starts/finishes so the host can lock other controls.</summary>
    public event EventHandler<bool>? BusyChanged;

    public ImportView()
    {
        InitializeComponent();

        Paint += UiTheme.PaintBorder;
        lblFileName.Paint += UiTheme.PaintBorder;
        UiTheme.StyleGrid(dgvPreview);
        UiTheme.StyleGrid(dgvMapping);
        dgvMapping.SelectionMode = DataGridViewSelectionMode.CellSelect;
        UiTheme.StyleSecondaryButton(btnBrowse);
        UiTheme.StylePrimaryButton(btnImport, UiTheme.Success, UiTheme.SuccessHover);

        cboDuplicateMode.Items.AddRange(
        [
            new DuplicateOption(DuplicateMode.Update, "อัปเดตข้อมูลเดิม"),
            new DuplicateOption(DuplicateMode.Skip, "ข้าม (ไม่แก้ไข)"),
            new DuplicateOption(DuplicateMode.Abort, "แจ้งเตือนและยกเลิก"),
        ]);
        cboDuplicateMode.SelectedIndex = 0;

        UpdateState();
    }

    public bool IsBusy => _busy;

    /// <summary>Called after the host successfully retrieves the table list.</summary>
    public void SetConnection(string connectionString, IReadOnlyList<TableInfo> tables)
    {
        _connectionString = connectionString;
        _tables = tables;
        _targetChosenByUser = false;

        _suppressEvents = true;
        cboTargetTable.BeginUpdate();
        cboTargetTable.Items.Clear();
        cboTargetTable.Items.AddRange(tables.ToArray<object>());
        cboTargetTable.EndUpdate();
        _suppressEvents = false;

        SetTarget(null);
        TrySuggestTarget();
        UpdateState();
    }

    /// <summary>Forgets the database connection but keeps the loaded Excel file.</summary>
    public void ClearConnection()
    {
        _connectionString = null;
        _tables = [];
        _targetChosenByUser = false;

        _suppressEvents = true;
        cboTargetTable.Items.Clear();
        _suppressEvents = false;

        SetTarget(null);
        UpdateState();
    }

    /// <summary>Clears the connection and the loaded Excel file.</summary>
    public void ResetAll()
    {
        ClearConnection();

        _workbook = null;
        _filePath = null;
        _suppressEvents = true;
        cboSheet.Items.Clear();
        _suppressEvents = false;

        dgvPreview.DataSource = null;
        lblFileName.Text = "ยังไม่ได้เลือกไฟล์ (.xlsx / .xls)";
        lblFileName.ForeColor = UiTheme.TextMuted;
        lblPreviewInfo.Text = string.Empty;
        progressImport.Value = 0;
        SetStatus("เลือกไฟล์ Excel เพื่อเริ่มต้น");
        UpdateState();
    }

    private DataTable? CurrentSheet => _workbook is not null && cboSheet.SelectedItem is string name && _workbook.Tables.Contains(name)
        ? _workbook.Tables[name]
        : null;

    // ───────────────────────────── Excel file ─────────────────────────────

    private async void btnBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "เลือกไฟล์ Excel",
            Filter = "Excel Files (*.xlsx;*.xls;*.xlsm)|*.xlsx;*.xls;*.xlsm|All Files (*.*)|*.*",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var path = dialog.FileName;
        SetStatus("กำลังอ่านไฟล์ Excel ...");
        UseWaitCursor = true;
        btnBrowse.Enabled = false;

        try
        {
            var workbook = await Task.Run(() => ExcelService.Read(path));
            if (workbook.Tables.Count == 0)
            {
                ShowWarning("ไม่พบ Sheet ในไฟล์ Excel นี้");
                return;
            }

            _workbook = workbook;
            _filePath = path;
            lblFileName.Text = $"📄  {Path.GetFileName(path)}";
            lblFileName.ForeColor = UiTheme.TextPrimary;

            _suppressEvents = true;
            cboSheet.Items.Clear();
            foreach (DataTable sheet in workbook.Tables)
            {
                cboSheet.Items.Add(sheet.TableName);
            }
            _suppressEvents = false;

            // Start with the first sheet that actually contains data.
            var first = workbook.Tables.Cast<DataTable>().FirstOrDefault(t => t.Rows.Count > 0) ?? workbook.Tables[0];
            cboSheet.SelectedItem = first.TableName;
        }
        catch (IOException ex)
        {
            ShowError($"ไม่สามารถเปิดไฟล์ได้ กรุณาตรวจสอบว่าไฟล์ไม่ได้ถูกล็อกโดยโปรแกรมอื่น\n\nรายละเอียด: {ex.Message}", "เปิดไฟล์ไม่สำเร็จ");
        }
        catch (Exception ex) when (ex is ExcelReaderException or InvalidDataException or NotSupportedException
                                       or ArgumentException or FormatException)
        {
            ShowError($"ไฟล์นี้ไม่ใช่ไฟล์ Excel ที่รองรับ หรือไฟล์เสียหาย / มีรหัสผ่าน\n\nรายละเอียด: {ex.Message}", "อ่านไฟล์ไม่สำเร็จ");
        }
        catch (Exception ex)
        {
            ShowError($"เกิดข้อผิดพลาดขณะอ่านไฟล์ Excel\n\nรายละเอียด: {ex.Message}", "อ่านไฟล์ไม่สำเร็จ");
        }
        finally
        {
            UseWaitCursor = false;
            btnBrowse.Enabled = !_busy;
            if (_workbook is null || _filePath != path)
            {
                SetStatus("เลือกไฟล์ Excel เพื่อเริ่มต้น");
            }
            UpdateState();
        }
    }

    private void cboSheet_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_suppressEvents)
        {
            return;
        }

        var sheet = CurrentSheet;
        dgvPreview.DataSource = sheet;
        lblPreviewInfo.Text = sheet is null
            ? string.Empty
            : $"{sheet.Rows.Count:N0} แถว · {ExcelService.GetDataColumns(sheet).Count} คอลัมน์";
        progressImport.Value = 0;

        if (!TrySuggestTarget())
        {
            RebuildMappings();
        }

        SetStatus(sheet is null ? string.Empty : $"อ่าน Sheet \"{sheet.TableName}\" แล้ว {sheet.Rows.Count:N0} แถว");
        UpdateState();
    }

    private void dgvPreview_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        if (dgvPreview.Columns.Contains(ExcelService.RowNumberColumn))
        {
            var rowColumn = dgvPreview.Columns[ExcelService.RowNumberColumn]!;
            rowColumn.HeaderText = "แถว";
            rowColumn.DefaultCellStyle.ForeColor = UiTheme.TextMuted;
            rowColumn.Width = 60;
            rowColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            rowColumn.Frozen = true;
        }

        foreach (DataGridViewColumn column in dgvPreview.Columns)
        {
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        dgvPreview.AutoSizeColumnsMode = dgvPreview.Columns.Count <= 7
            ? DataGridViewAutoSizeColumnsMode.Fill
            : DataGridViewAutoSizeColumnsMode.DisplayedCells;

        HighlightPreviewColumns();
    }

    // ───────────────────────────── Target table & schema ─────────────────────────────

    private bool TrySuggestTarget()
    {
        if (_targetChosenByUser || _tables.Count == 0 || CurrentSheet is not { } sheet)
        {
            return false;
        }

        var suggestion = ColumnMapper.SuggestTable(_tables, sheet.TableName, Path.GetFileNameWithoutExtension(_filePath ?? string.Empty));
        if (suggestion is null || suggestion == _targetTable)
        {
            return false;
        }

        _suppressEvents = true;
        cboTargetTable.SelectedItem = suggestion;
        _suppressEvents = false;

        SetTarget(suggestion);
        SetStatus($"ตรวจพบตารางปลายทางอัตโนมัติ: {suggestion.FullName} (เปลี่ยนได้)");
        return true;
    }

    private void cboTargetTable_SelectionChangeCommitted(object? sender, EventArgs e)
    {
        if (_suppressEvents)
        {
            return;
        }

        _targetChosenByUser = true;
        SetTarget(cboTargetTable.SelectedItem as TableInfo);
    }

    private async void SetTarget(TableInfo? table)
    {
        _targetTable = table;
        _schema = null;
        var request = ++_schemaRequest;

        if (table is null || _connectionString is null)
        {
            if (table is null && cboTargetTable.SelectedIndex != -1)
            {
                _suppressEvents = true;
                cboTargetTable.SelectedIndex = -1;
                _suppressEvents = false;
            }
            RebuildMappings();
            return;
        }

        lblMappingInfo.Text = "กำลังอ่านโครงสร้างตาราง ...";
        UseWaitCursor = true;
        try
        {
            var schema = await SchemaService.GetColumnsAsync(_connectionString, table);
            if (request != _schemaRequest)
            {
                return; // a newer selection superseded this one
            }

            _schema = schema;
            RebuildMappings();

            var pk = schema.Where(c => c.IsPrimaryKey).Select(c => c.Name).ToList();
            var identity = schema.Where(c => c.IsIdentity).Select(c => c.Name).ToList();
            SetStatus($"โครงสร้าง {table.FullName}: {schema.Count} คอลัมน์"
                      + $" · PK: {(pk.Count > 0 ? string.Join(", ", pk) : "ไม่มี")}"
                      + $" · Identity: {(identity.Count > 0 ? string.Join(", ", identity) : "ไม่มี")}");
        }
        catch (Exception ex)
        {
            if (request != _schemaRequest)
            {
                return;
            }
            RebuildMappings();
            ShowError(DescribeError(ex), "อ่านโครงสร้างตารางไม่สำเร็จ");
        }
        finally
        {
            if (request == _schemaRequest)
            {
                UseWaitCursor = false;
                UpdateState();
            }
        }
    }

    // ───────────────────────────── Column mapping ─────────────────────────────

    private void RebuildMappings()
    {
        var headers = CurrentSheet is { } sheet ? ExcelService.GetDataColumns(sheet) : [];
        _mappings = _schema is null ? [] : ColumnMapper.AutoMap(_schema, headers);

        _suppressEvents = true;
        dgvMapping.Rows.Clear();

        foreach (var mapping in _mappings)
        {
            var column = mapping.Column;
            var index = dgvMapping.Rows.Add(column.Name, column.TypeDisplay, column.AttributesDisplay, null, null, mapping.IsKey);
            var row = dgvMapping.Rows[index];
            row.Tag = mapping;

            if (!column.IsWritable)
            {
                row.Cells[colMapExcel.Index] = new DataGridViewTextBoxCell { Value = column.IsIdentity ? IdentityText : ComputedText };
                row.Cells[colMapExcel.Index].ReadOnly = true;
                row.Cells[colMapConstant.Index].ReadOnly = true;
                row.Cells[colMapKey.Index].ReadOnly = true;
                row.DefaultCellStyle.ForeColor = UiTheme.TextMuted;
                continue;
            }

            var cell = (DataGridViewComboBoxCell)row.Cells[colMapExcel.Index];
            cell.Items.Add(NotImportedItem);
            if (column.CanAutoGenerate)
            {
                cell.Items.Add(AutoIncrementItem);
            }
            foreach (var header in headers)
            {
                cell.Items.Add(header);
            }

            cell.Value = mapping.Source switch
            {
                MappingSource.Excel => mapping.ExcelColumn,
                MappingSource.AutoIncrement => AutoIncrementItem,
                _ => NotImportedItem,
            };
        }

        _suppressEvents = false;
        RefreshMappingStyles();
    }

    private void dgvMapping_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        // Apply combo/checkbox changes immediately instead of waiting for the cell to lose focus.
        if (dgvMapping.IsCurrentCellDirty && dgvMapping.CurrentCell is DataGridViewComboBoxCell or DataGridViewCheckBoxCell)
        {
            dgvMapping.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    private void dgvMapping_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (_suppressEvents || e.RowIndex < 0 || dgvMapping.Rows[e.RowIndex].Tag is not ColumnMapping mapping)
        {
            return;
        }

        var value = dgvMapping.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

        if (e.ColumnIndex == colMapExcel.Index)
        {
            var text = value as string;
            mapping.AutoIncrement = text == AutoIncrementItem;
            mapping.ExcelColumn = text is null or NotImportedItem or AutoIncrementItem ? null : text;
        }
        else if (e.ColumnIndex == colMapConstant.Index)
        {
            mapping.ConstantValue = string.IsNullOrWhiteSpace(value as string) ? null : ((string)value!).Trim();
        }
        else if (e.ColumnIndex == colMapKey.Index)
        {
            mapping.IsKey = value is true;
        }

        RefreshMappingStyles();
    }

    private void dgvMapping_DataError(object? sender, DataGridViewDataErrorEventArgs e)
    {
        // Combo values are always taken from the cell's own item list; ignore transient errors.
        e.ThrowException = false;
    }

    private void RefreshMappingStyles()
    {
        foreach (DataGridViewRow row in dgvMapping.Rows)
        {
            if (row.Tag is not ColumnMapping m || !m.Column.IsWritable)
            {
                continue;
            }

            var missing = m.Source == MappingSource.None && !m.Column.IsNullable && !m.Column.HasDefault;
            row.DefaultCellStyle.BackColor = missing ? UiTheme.Warning : Color.Empty;
            row.DefaultCellStyle.ForeColor = missing ? UiTheme.WarningText
                : m.Source == MappingSource.None ? UiTheme.TextMuted : Color.Empty;
        }

        var mapped = _mappings.Count(m => m.Source == MappingSource.Excel);
        var keys = _mappings.Where(m => m.IsKey).Select(m => m.Column.Name).ToList();
        lblMappingInfo.Text = _schema is null
            ? (_targetTable is null ? "เลือกตารางปลายทางเพื่อจับคู่คอลัมน์" : string.Empty)
            : $"จับคู่แล้ว {mapped} คอลัมน์ · คีย์ตรวจซ้ำ: {(keys.Count > 0 ? string.Join(", ", keys) : "ไม่มี")}";

        HighlightPreviewColumns();
        UpdateState();
    }

    private void HighlightPreviewColumns()
    {
        var mapped = _mappings.Where(m => m.Source == MappingSource.Excel).Select(m => m.ExcelColumn).ToHashSet();
        foreach (DataGridViewColumn column in dgvPreview.Columns)
        {
            if (column.Name == ExcelService.RowNumberColumn)
            {
                continue;
            }
            column.DefaultCellStyle.ForeColor = mapped.Count == 0 || mapped.Contains(column.Name) ? Color.Empty : UiTheme.TextMuted;
            column.HeaderCell.Style.ForeColor = mapped.Count == 0 || mapped.Contains(column.Name) ? Color.Empty : UiTheme.TextMuted;
        }
    }

    // ───────────────────────────── Import ─────────────────────────────

    private async void btnImport_Click(object? sender, EventArgs e)
    {
        dgvMapping.EndEdit();

        if (_connectionString is null || _targetTable is null || _schema is null || CurrentSheet is not { } sheet)
        {
            ShowWarning("กรุณาเชื่อมต่อฐานข้อมูล เลือกไฟล์ Excel และเลือกตารางปลายทางให้ครบก่อน Import");
            return;
        }

        ImportPlan plan;
        try
        {
            plan = ImportService.Prepare(_targetTable, _mappings, sheet);
        }
        catch (ImportValidationException ex)
        {
            ShowWarning("ข้อมูลยังไม่พร้อมสำหรับ Import กรุณาแก้ไขรายการต่อไปนี้:\n\n• " + string.Join("\n• ", ex.Errors));
            return;
        }

        var mode = ((DuplicateOption)cboDuplicateMode.SelectedItem!).Mode;
        SetBusy(true);

        try
        {
            SetStatus("กำลังตรวจสอบข้อมูลซ้ำ ...");
            var existing = await ImportService.FindExistingAsync(_connectionString, plan);

            if (existing.Count > 0 && mode == DuplicateMode.Abort)
            {
                SetStatus($"ยกเลิก: พบข้อมูลซ้ำ {existing.Count:N0} แถว");
                ShowWarning($"พบข้อมูลที่มีอยู่แล้วในตาราง {existing.Count:N0} แถว:\n\n{FormatExisting(existing)}\n\n"
                            + "หากต้องการนำเข้าต่อ ให้เปลี่ยนตัวเลือก \"ถ้าพบข้อมูลซ้ำ\" เป็น \"อัปเดตข้อมูลเดิม\" หรือ \"ข้าม\"");
                return;
            }

            if (!ConfirmImport(plan, existing, mode))
            {
                SetStatus("ยกเลิกการ Import");
                return;
            }

            progressImport.Maximum = plan.Rows.Count;
            progressImport.Value = 0;
            var progress = new Progress<int>(done =>
            {
                progressImport.Value = Math.Min(done, progressImport.Maximum);
                SetStatus($"กำลังนำเข้า {done:N0} / {plan.Rows.Count:N0} แถว ...");
            });

            var result = await ImportService.ImportAsync(_connectionString, plan, mode, progress);

            progressImport.Value = progressImport.Maximum;
            var summary = $"เพิ่มใหม่ {result.Inserted:N0} · อัปเดต {result.Updated:N0} · ข้าม {result.Skipped:N0} แถว";
            SetStatus($"✔ Import สำเร็จ — {summary}");
            MessageBox.Show(this, $"นำเข้าข้อมูลเข้าตาราง {plan.Table.FullName} สำเร็จ\n\n• เพิ่มใหม่: {result.Inserted:N0} แถว\n• อัปเดต: {result.Updated:N0} แถว\n• ข้าม: {result.Skipped:N0} แถว",
                "Import สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            progressImport.Value = 0;
            SetStatus("✖ Import ไม่สำเร็จ — ยกเลิก (Rollback) ข้อมูลทั้งหมดแล้ว");
            ShowError(DescribeError(ex) + "\n\nระบบได้ยกเลิก (Rollback) การนำเข้าทั้งหมดแล้ว ไม่มีข้อมูลใดถูกบันทึก", "Import ไม่สำเร็จ");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private bool ConfirmImport(ImportPlan plan, List<ExistingRow> existing, DuplicateMode mode)
    {
        var newRows = plan.Rows.Count - existing.Count;
        var message = $"ยืนยันการนำเข้า {plan.Rows.Count:N0} แถว เข้าตาราง {plan.Table.FullName}\n\n"
                      + $"• ข้อมูลใหม่ (Insert): {newRows:N0} แถว\n";

        if (!plan.HasKeys)
        {
            message += "\n⚠ ไม่ได้กำหนด \"คีย์ตรวจซ้ำ\" — ทุกแถวจะถูกเพิ่มเป็นข้อมูลใหม่โดยไม่ตรวจสอบข้อมูลซ้ำ\n";
        }
        else if (existing.Count > 0)
        {
            var action = mode == DuplicateMode.Update ? "อัปเดตข้อมูลเดิม" : "ข้าม ไม่แก้ไข";
            message += $"• พบข้อมูลเดิมในระบบ: {existing.Count:N0} แถว → {action}\n\n{FormatExisting(existing)}\n";
        }

        var autoColumns = plan.Columns.Where(c => c.Source == MappingSource.AutoIncrement).Select(c => c.Column.Name).ToList();
        if (autoColumns.Count > 0)
        {
            message += $"\nคอลัมน์ {string.Join(", ", autoColumns)} จะถูกสร้างอัตโนมัติ (MAX + 1)\n";
        }

        message += "\nการนำเข้าทำงานภายใน Transaction หากเกิดข้อผิดพลาดจะยกเลิกทั้งหมด";

        return MessageBox.Show(this, message, "ยืนยันการ Import", MessageBoxButtons.YesNo,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
    }

    private static string FormatExisting(List<ExistingRow> existing)
    {
        const int shown = 8;
        var lines = existing.Take(shown).Select(r => $"   แถว {r.ExcelRow}: {r.KeyDisplay}");
        var more = existing.Count > shown ? $"\n   ... และอีก {existing.Count - shown:N0} แถว" : string.Empty;
        return string.Join("\n", lines) + more;
    }

    private string DescribeError(Exception ex)
    {
        var builder = new SqlConnectionStringBuilder(_connectionString ?? string.Empty);

        string Friendly(Exception inner) => inner is SqlException sql
            ? DatabaseService.GetFriendlyMessage(sql, builder.DataSource, builder.InitialCatalog, builder.UserID)
            : inner.Message;

        return ex switch
        {
            DuplicateRowException dup => $"พบข้อมูลซ้ำในระบบที่แถว Excel {dup.ExcelRow} ({dup.KeyDisplay})",
            ImportRowException row => $"เกิดข้อผิดพลาดที่แถว Excel {row.ExcelRow}" +
                                      (string.IsNullOrEmpty(row.KeyDisplay) ? string.Empty : $" ({row.KeyDisplay})") +
                                      $"\n\n{Friendly(row.InnerException!)}",
            _ => Friendly(ex),
        };
    }

    // ───────────────────────────── State helpers ─────────────────────────────

    private void SetBusy(bool busy)
    {
        _busy = busy;
        UseWaitCursor = busy;
        UpdateState();
        BusyChanged?.Invoke(this, busy);
    }

    private void UpdateState()
    {
        var connected = _connectionString is not null;
        btnBrowse.Enabled = !_busy;
        cboSheet.Enabled = !_busy && cboSheet.Items.Count > 0;
        cboTargetTable.Enabled = !_busy && connected;
        cboDuplicateMode.Enabled = !_busy;
        // Not ReadOnly: toggling DataGridView.ReadOnly resets every cell's ReadOnly flag.
        dgvMapping.Enabled = !_busy;
        btnImport.Enabled = !_busy && connected && _schema is not null && CurrentSheet is { Rows.Count: > 0 }
                            && _mappings.Any(m => m.Source == MappingSource.Excel);
        btnImport.Text = _busy ? "กำลัง Import ..." : "⬆  Import ข้อมูล";

        if (!connected && !_busy)
        {
            lblMappingInfo.Text = "กรุณาเชื่อมต่อฐานข้อมูล (Retrieve Table) ก่อนเลือกตารางปลายทาง";
        }
    }

    private void SetStatus(string message) => lblImportStatus.Text = message;

    private void ShowError(string message, string title) =>
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    private void ShowWarning(string message) =>
        MessageBox.Show(this, message, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
