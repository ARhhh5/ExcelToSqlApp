using System.Text.Json;
using ExcelToSqlApp.Models;
using ExcelToSqlApp.Services;
using Microsoft.Data.SqlClient;

namespace ExcelToSqlApp;

public partial class MainForm : Form
{
    private const char PasswordMask = '●';
    private static readonly Color CardBorderColor = Color.FromArgb(226, 232, 240);

    private List<TableInfo> _tables = [];

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        LoadClients();
        UpdateTableCount(0);
    }

    private void LoadClients()
    {
        try
        {
            var clients = ClientRepository.Load();

            cboClient.Items.Clear();
            cboClient.Items.AddRange(clients.ToArray<object>());
            cboClient.SelectedIndex = -1;

            SetStatus(clients.Count > 0
                ? $"โหลดโปรไฟล์ลูกค้า {clients.Count} รายการ — กรุณาเลือกลูกค้า"
                : $"ไม่พบโปรไฟล์ลูกค้าในไฟล์ {ClientRepository.FileName}");
        }
        catch (FileNotFoundException)
        {
            ShowWarning($"ไม่พบไฟล์ {ClientRepository.FileName}\n\nตำแหน่งที่ค้นหา:\n{ClientRepository.FilePath}\n\nคุณยังสามารถกรอกข้อมูลการเชื่อมต่อเองได้");
            SetStatus($"ไม่พบไฟล์ {ClientRepository.FileName}");
        }
        catch (JsonException ex)
        {
            ShowWarning($"รูปแบบไฟล์ {ClientRepository.FileName} ไม่ถูกต้อง กรุณาตรวจสอบไฟล์\n\nรายละเอียด: {ex.Message}");
            SetStatus($"อ่านไฟล์ {ClientRepository.FileName} ไม่สำเร็จ");
        }
        catch (Exception ex)
        {
            ShowWarning($"ไม่สามารถอ่านไฟล์ {ClientRepository.FileName} ได้\n\nรายละเอียด: {ex.Message}");
            SetStatus($"อ่านไฟล์ {ClientRepository.FileName} ไม่สำเร็จ");
        }
    }

    private void cboClient_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cboClient.SelectedItem is not ClientProfile client)
        {
            return;
        }

        txtServer.Text = client.Server;
        txtDatabase.Text = client.Database;
        txtUser.Text = client.User;
        txtPassword.Text = client.Password;

        // Tables shown belong to the previous client; drop them to avoid confusion.
        ResetTables();
        SetStatus($"เลือกลูกค้า: {client.ClientName}");
    }

    private async void btnRetrieve_Click(object? sender, EventArgs e)
    {
        var server = txtServer.Text.Trim();
        var database = txtDatabase.Text.Trim();
        var user = txtUser.Text.Trim();
        var password = txtPassword.Text;

        if (!ValidateInputs(server, database, user, password))
        {
            return;
        }

        SetBusy(true);
        SetStatus($"กำลังเชื่อมต่อ {server} / {database} ...");

        try
        {
            var connectionString = DatabaseService.BuildConnectionString(server, database, user, password);
            _tables = await DatabaseService.GetTablesAsync(connectionString);

            txtSearch.Clear();
            ApplyFilter();
            SetStatus($"เชื่อมต่อสำเร็จ — พบ {_tables.Count:N0} ตารางในฐานข้อมูล {database}");

            if (_tables.Count == 0)
            {
                MessageBox.Show(this, $"เชื่อมต่อสำเร็จ แต่ไม่พบตารางในฐานข้อมูล \"{database}\"",
                    "ไม่พบตาราง", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (SqlException ex)
        {
            ResetTables();
            SetStatus("เชื่อมต่อไม่สำเร็จ");
            ShowError(DatabaseService.GetFriendlyMessage(ex, server, database, user), "เชื่อมต่อฐานข้อมูลไม่สำเร็จ");
        }
        catch (ArgumentException ex)
        {
            ResetTables();
            SetStatus("ข้อมูลการเชื่อมต่อไม่ถูกต้อง");
            ShowError($"ข้อมูลการเชื่อมต่อไม่ถูกต้อง กรุณาตรวจสอบค่าที่กรอก\n\nรายละเอียด: {ex.Message}", "ข้อมูลไม่ถูกต้อง");
        }
        catch (Exception ex)
        {
            ResetTables();
            SetStatus("เกิดข้อผิดพลาด");
            ShowError($"เกิดข้อผิดพลาดที่ไม่คาดคิด\n\nรายละเอียด: {ex.Message}", "เกิดข้อผิดพลาด");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void btnClear_Click(object? sender, EventArgs e)
    {
        cboClient.SelectedIndex = -1;
        txtServer.Clear();
        txtDatabase.Clear();
        txtUser.Clear();
        txtPassword.Clear();
        chkShowPassword.Checked = false;
        txtSearch.Clear();
        ResetTables();

        SetStatus("ล้างข้อมูลเรียบร้อย");
        cboClient.Focus();
    }

    private void txtSearch_TextChanged(object? sender, EventArgs e) => ApplyFilter();

    private void chkShowPassword_CheckedChanged(object? sender, EventArgs e)
    {
        txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : PasswordMask;
    }

    private void Card_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Control card)
        {
            return;
        }

        using var pen = new Pen(CardBorderColor);
        e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
    }

    private bool ValidateInputs(string server, string database, string user, string password)
    {
        (string Value, string Name, Control Field)[] required =
        [
            (server, "Server", txtServer),
            (database, "Database", txtDatabase),
            (user, "User", txtUser),
            (password, "Password", txtPassword),
        ];

        foreach (var (value, name, field) in required)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show(this, $"กรุณากรอก {name}", "ข้อมูลไม่ครบ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                field.Focus();
                return false;
            }
        }

        return true;
    }

    private void ApplyFilter()
    {
        var keyword = txtSearch.Text.Trim();
        var filtered = string.IsNullOrEmpty(keyword)
            ? _tables
            : _tables.Where(t => t.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

        dgvTables.SuspendLayout();
        dgvTables.Rows.Clear();
        for (var i = 0; i < filtered.Count; i++)
        {
            dgvTables.Rows.Add(i + 1, filtered[i].Schema, filtered[i].Name);
        }
        dgvTables.ResumeLayout();

        UpdateTableCount(filtered.Count);
    }

    private void ResetTables()
    {
        _tables = [];
        dgvTables.Rows.Clear();
        UpdateTableCount(0);
    }

    private void UpdateTableCount(int shown)
    {
        if (_tables.Count == 0)
        {
            lblTableCount.Text = "ยังไม่มีข้อมูล";
        }
        else if (shown == _tables.Count)
        {
            lblTableCount.Text = $"ทั้งหมด {_tables.Count:N0} ตาราง";
        }
        else
        {
            lblTableCount.Text = $"แสดง {shown:N0} จาก {_tables.Count:N0} ตาราง";
        }
    }

    private void SetBusy(bool busy)
    {
        UseWaitCursor = busy;
        btnRetrieve.Enabled = !busy;
        btnClear.Enabled = !busy;
        cboClient.Enabled = !busy;
        btnRetrieve.Text = busy ? "กำลังเชื่อมต่อ..." : "Retrieve Table";
    }

    private void SetStatus(string message) => lblStatus.Text = message;

    private void ShowError(string message, string title) =>
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    private void ShowWarning(string message) =>
        MessageBox.Show(this, message, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
