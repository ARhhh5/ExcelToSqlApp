namespace ExcelToSqlApp;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        DataGridViewCellStyle alternatingRowStyle = new DataGridViewCellStyle();
        DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
        DataGridViewCellStyle cellStyle = new DataGridViewCellStyle();
        DataGridViewCellStyle numberCellStyle = new DataGridViewCellStyle();
        pnlHeader = new Panel();
        lblSubtitle = new Label();
        lblTitle = new Label();
        statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel();
        tlpMain = new TableLayoutPanel();
        pnlConnection = new Panel();
        btnClear = new Button();
        btnRetrieve = new Button();
        chkShowPassword = new CheckBox();
        txtPassword = new TextBox();
        lblPassword = new Label();
        txtUser = new TextBox();
        lblUser = new Label();
        txtDatabase = new TextBox();
        lblDatabase = new Label();
        txtServer = new TextBox();
        lblServer = new Label();
        cboClient = new ComboBox();
        lblClient = new Label();
        lblConnectionTitle = new Label();
        pnlTables = new Panel();
        dgvTables = new DataGridView();
        colNo = new DataGridViewTextBoxColumn();
        colSchema = new DataGridViewTextBoxColumn();
        colTableName = new DataGridViewTextBoxColumn();
        pnlTablesHeader = new Panel();
        txtSearch = new TextBox();
        lblTableCount = new Label();
        lblTablesTitle = new Label();
        pnlRight = new Panel();
        pnlTabBar = new Panel();
        btnTabTables = new Button();
        btnTabImport = new Button();
        importView = new Controls.ImportView();
        pnlHeader.SuspendLayout();
        statusStrip.SuspendLayout();
        tlpMain.SuspendLayout();
        pnlConnection.SuspendLayout();
        pnlRight.SuspendLayout();
        pnlTabBar.SuspendLayout();
        pnlTables.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvTables).BeginInit();
        pnlTablesHeader.SuspendLayout();
        SuspendLayout();
        //
        // pnlHeader
        //
        pnlHeader.BackColor = Color.FromArgb(30, 41, 59);
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(1320, 78);
        pnlHeader.TabIndex = 0;
        //
        // lblSubtitle
        //
        lblSubtitle.AutoSize = true;
        lblSubtitle.ForeColor = Color.FromArgb(148, 163, 184);
        lblSubtitle.Location = new Point(26, 46);
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Size = new Size(300, 19);
        lblSubtitle.TabIndex = 1;
        lblSubtitle.Text = "เชื่อมต่อฐานข้อมูลลูกค้า ดูรายชื่อตาราง และนำเข้าข้อมูลจากไฟล์ Excel";
        //
        // lblTitle
        //
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(22, 10);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(231, 30);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Import Excel to SQL";
        //
        // statusStrip
        //
        statusStrip.BackColor = Color.White;
        statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
        statusStrip.Location = new Point(0, 794);
        statusStrip.Name = "statusStrip";
        statusStrip.Padding = new Padding(16, 0, 16, 0);
        statusStrip.Size = new Size(1320, 26);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 2;
        //
        // lblStatus
        //
        lblStatus.ForeColor = Color.FromArgb(71, 85, 105);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(40, 21);
        lblStatus.Text = "พร้อมใช้งาน";
        //
        // tlpMain
        //
        tlpMain.ColumnCount = 2;
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350F));
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpMain.Controls.Add(pnlConnection, 0, 0);
        tlpMain.Controls.Add(pnlRight, 1, 0);
        tlpMain.Dock = DockStyle.Fill;
        tlpMain.Location = new Point(0, 78);
        tlpMain.Name = "tlpMain";
        tlpMain.Padding = new Padding(20);
        tlpMain.RowCount = 1;
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpMain.Size = new Size(1320, 716);
        tlpMain.TabIndex = 1;
        //
        // pnlConnection
        //
        pnlConnection.BackColor = Color.White;
        pnlConnection.Controls.Add(btnClear);
        pnlConnection.Controls.Add(btnRetrieve);
        pnlConnection.Controls.Add(chkShowPassword);
        pnlConnection.Controls.Add(txtPassword);
        pnlConnection.Controls.Add(lblPassword);
        pnlConnection.Controls.Add(txtUser);
        pnlConnection.Controls.Add(lblUser);
        pnlConnection.Controls.Add(txtDatabase);
        pnlConnection.Controls.Add(lblDatabase);
        pnlConnection.Controls.Add(txtServer);
        pnlConnection.Controls.Add(lblServer);
        pnlConnection.Controls.Add(cboClient);
        pnlConnection.Controls.Add(lblClient);
        pnlConnection.Controls.Add(lblConnectionTitle);
        pnlConnection.Dock = DockStyle.Fill;
        pnlConnection.Location = new Point(20, 20);
        pnlConnection.Margin = new Padding(0, 0, 10, 0);
        pnlConnection.Name = "pnlConnection";
        pnlConnection.Size = new Size(340, 676);
        pnlConnection.TabIndex = 0;
        pnlConnection.Paint += Card_Paint;
        //
        // lblConnectionTitle
        //
        lblConnectionTitle.AutoSize = true;
        lblConnectionTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        lblConnectionTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblConnectionTitle.Location = new Point(20, 18);
        lblConnectionTitle.Name = "lblConnectionTitle";
        lblConnectionTitle.Size = new Size(120, 21);
        lblConnectionTitle.TabIndex = 0;
        lblConnectionTitle.Text = "การเชื่อมต่อฐานข้อมูล";
        //
        // lblClient
        //
        lblClient.AutoSize = true;
        lblClient.ForeColor = Color.FromArgb(71, 85, 105);
        lblClient.Location = new Point(20, 58);
        lblClient.Name = "lblClient";
        lblClient.Size = new Size(60, 19);
        lblClient.TabIndex = 1;
        lblClient.Text = "ชื่อลูกค้า";
        //
        // cboClient
        //
        cboClient.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        cboClient.DropDownStyle = ComboBoxStyle.DropDownList;
        cboClient.FlatStyle = FlatStyle.Flat;
        cboClient.Font = new Font("Segoe UI", 10.5F);
        cboClient.FormattingEnabled = true;
        cboClient.Location = new Point(20, 80);
        cboClient.Name = "cboClient";
        cboClient.Size = new Size(300, 27);
        cboClient.TabIndex = 2;
        cboClient.SelectedIndexChanged += cboClient_SelectedIndexChanged;
        //
        // lblServer
        //
        lblServer.AutoSize = true;
        lblServer.ForeColor = Color.FromArgb(71, 85, 105);
        lblServer.Location = new Point(20, 122);
        lblServer.Name = "lblServer";
        lblServer.Size = new Size(48, 19);
        lblServer.TabIndex = 3;
        lblServer.Text = "Server";
        //
        // txtServer
        //
        txtServer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtServer.BorderStyle = BorderStyle.FixedSingle;
        txtServer.Font = new Font("Segoe UI", 10.5F);
        txtServer.Location = new Point(20, 144);
        txtServer.Name = "txtServer";
        txtServer.PlaceholderText = "เช่น 192.168.1.10\\SQLEXPRESS";
        txtServer.Size = new Size(300, 26);
        txtServer.TabIndex = 4;
        //
        // lblDatabase
        //
        lblDatabase.AutoSize = true;
        lblDatabase.ForeColor = Color.FromArgb(71, 85, 105);
        lblDatabase.Location = new Point(20, 184);
        lblDatabase.Name = "lblDatabase";
        lblDatabase.Size = new Size(65, 19);
        lblDatabase.TabIndex = 5;
        lblDatabase.Text = "Database";
        //
        // txtDatabase
        //
        txtDatabase.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtDatabase.BorderStyle = BorderStyle.FixedSingle;
        txtDatabase.Font = new Font("Segoe UI", 10.5F);
        txtDatabase.Location = new Point(20, 206);
        txtDatabase.Name = "txtDatabase";
        txtDatabase.Size = new Size(300, 26);
        txtDatabase.TabIndex = 6;
        //
        // lblUser
        //
        lblUser.AutoSize = true;
        lblUser.ForeColor = Color.FromArgb(71, 85, 105);
        lblUser.Location = new Point(20, 246);
        lblUser.Name = "lblUser";
        lblUser.Size = new Size(36, 19);
        lblUser.TabIndex = 7;
        lblUser.Text = "User";
        //
        // txtUser
        //
        txtUser.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtUser.BorderStyle = BorderStyle.FixedSingle;
        txtUser.Font = new Font("Segoe UI", 10.5F);
        txtUser.Location = new Point(20, 268);
        txtUser.Name = "txtUser";
        txtUser.Size = new Size(300, 26);
        txtUser.TabIndex = 8;
        //
        // lblPassword
        //
        lblPassword.AutoSize = true;
        lblPassword.ForeColor = Color.FromArgb(71, 85, 105);
        lblPassword.Location = new Point(20, 308);
        lblPassword.Name = "lblPassword";
        lblPassword.Size = new Size(67, 19);
        lblPassword.TabIndex = 9;
        lblPassword.Text = "Password";
        //
        // txtPassword
        //
        txtPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtPassword.BorderStyle = BorderStyle.FixedSingle;
        txtPassword.Font = new Font("Segoe UI", 10.5F);
        txtPassword.Location = new Point(20, 330);
        txtPassword.Name = "txtPassword";
        txtPassword.PasswordChar = '●';
        txtPassword.Size = new Size(300, 26);
        txtPassword.TabIndex = 10;
        //
        // chkShowPassword
        //
        chkShowPassword.AutoSize = true;
        chkShowPassword.Cursor = Cursors.Hand;
        chkShowPassword.ForeColor = Color.FromArgb(100, 116, 139);
        chkShowPassword.Location = new Point(20, 362);
        chkShowPassword.Name = "chkShowPassword";
        chkShowPassword.Size = new Size(110, 23);
        chkShowPassword.TabIndex = 11;
        chkShowPassword.Text = "แสดงรหัสผ่าน";
        chkShowPassword.UseVisualStyleBackColor = true;
        chkShowPassword.CheckedChanged += chkShowPassword_CheckedChanged;
        //
        // btnRetrieve
        //
        btnRetrieve.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnRetrieve.BackColor = Color.FromArgb(37, 99, 235);
        btnRetrieve.Cursor = Cursors.Hand;
        btnRetrieve.FlatAppearance.BorderSize = 0;
        btnRetrieve.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 64, 175);
        btnRetrieve.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
        btnRetrieve.FlatStyle = FlatStyle.Flat;
        btnRetrieve.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        btnRetrieve.ForeColor = Color.White;
        btnRetrieve.Location = new Point(20, 404);
        btnRetrieve.Name = "btnRetrieve";
        btnRetrieve.Size = new Size(300, 42);
        btnRetrieve.TabIndex = 12;
        btnRetrieve.Text = "Retrieve Table";
        btnRetrieve.UseVisualStyleBackColor = false;
        btnRetrieve.Click += btnRetrieve_Click;
        //
        // btnClear
        //
        btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnClear.BackColor = Color.White;
        btnClear.Cursor = Cursors.Hand;
        btnClear.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        btnClear.FlatAppearance.MouseDownBackColor = Color.FromArgb(226, 232, 240);
        btnClear.FlatAppearance.MouseOverBackColor = Color.FromArgb(241, 245, 249);
        btnClear.FlatStyle = FlatStyle.Flat;
        btnClear.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        btnClear.ForeColor = Color.FromArgb(51, 65, 85);
        btnClear.Location = new Point(20, 454);
        btnClear.Name = "btnClear";
        btnClear.Size = new Size(300, 42);
        btnClear.TabIndex = 13;
        btnClear.Text = "Clear Data";
        btnClear.UseVisualStyleBackColor = false;
        btnClear.Click += btnClear_Click;
        //
        // pnlRight
        //
        pnlRight.Controls.Add(pnlTables);
        pnlRight.Controls.Add(importView);
        pnlRight.Controls.Add(pnlTabBar);
        pnlRight.Dock = DockStyle.Fill;
        pnlRight.Location = new Point(380, 20);
        pnlRight.Margin = new Padding(10, 0, 0, 0);
        pnlRight.Name = "pnlRight";
        pnlRight.Size = new Size(920, 676);
        pnlRight.TabIndex = 1;
        //
        // pnlTabBar
        //
        pnlTabBar.Controls.Add(btnTabImport);
        pnlTabBar.Controls.Add(btnTabTables);
        pnlTabBar.Dock = DockStyle.Top;
        pnlTabBar.Location = new Point(0, 0);
        pnlTabBar.Name = "pnlTabBar";
        pnlTabBar.Size = new Size(920, 50);
        pnlTabBar.TabIndex = 0;
        //
        // btnTabTables
        //
        btnTabTables.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        btnTabTables.Location = new Point(0, 0);
        btnTabTables.Name = "btnTabTables";
        btnTabTables.Size = new Size(200, 38);
        btnTabTables.TabIndex = 0;
        btnTabTables.Text = "🗂  ตารางในฐานข้อมูล";
        btnTabTables.Click += btnTabTables_Click;
        //
        // btnTabImport
        //
        btnTabImport.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        btnTabImport.Location = new Point(208, 0);
        btnTabImport.Name = "btnTabImport";
        btnTabImport.Size = new Size(200, 38);
        btnTabImport.TabIndex = 1;
        btnTabImport.Text = "📥  Import Excel";
        btnTabImport.Click += btnTabImport_Click;
        //
        // importView
        //
        importView.Dock = DockStyle.Fill;
        importView.Location = new Point(0, 50);
        importView.Name = "importView";
        importView.Size = new Size(920, 626);
        importView.TabIndex = 2;
        importView.Visible = false;
        //
        // pnlTables
        //
        pnlTables.BackColor = Color.White;
        pnlTables.Controls.Add(dgvTables);
        pnlTables.Controls.Add(pnlTablesHeader);
        pnlTables.Dock = DockStyle.Fill;
        pnlTables.Location = new Point(0, 50);
        pnlTables.Margin = new Padding(0);
        pnlTables.Name = "pnlTables";
        pnlTables.Padding = new Padding(20, 18, 20, 20);
        pnlTables.Size = new Size(920, 626);
        pnlTables.TabIndex = 1;
        pnlTables.Paint += Card_Paint;
        //
        // pnlTablesHeader
        //
        pnlTablesHeader.Controls.Add(txtSearch);
        pnlTablesHeader.Controls.Add(lblTableCount);
        pnlTablesHeader.Controls.Add(lblTablesTitle);
        pnlTablesHeader.Dock = DockStyle.Top;
        pnlTablesHeader.Location = new Point(20, 18);
        pnlTablesHeader.Name = "pnlTablesHeader";
        pnlTablesHeader.Size = new Size(624, 84);
        pnlTablesHeader.TabIndex = 0;
        //
        // lblTablesTitle
        //
        lblTablesTitle.AutoSize = true;
        lblTablesTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        lblTablesTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTablesTitle.Location = new Point(0, 0);
        lblTablesTitle.Name = "lblTablesTitle";
        lblTablesTitle.Size = new Size(90, 21);
        lblTablesTitle.TabIndex = 0;
        lblTablesTitle.Text = "รายชื่อตาราง";
        //
        // lblTableCount
        //
        lblTableCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblTableCount.ForeColor = Color.FromArgb(100, 116, 139);
        lblTableCount.Location = new Point(384, 2);
        lblTableCount.Name = "lblTableCount";
        lblTableCount.Size = new Size(240, 19);
        lblTableCount.TabIndex = 1;
        lblTableCount.Text = "ยังไม่มีข้อมูล";
        lblTableCount.TextAlign = ContentAlignment.MiddleRight;
        //
        // txtSearch
        //
        txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtSearch.BorderStyle = BorderStyle.FixedSingle;
        txtSearch.Font = new Font("Segoe UI", 10.5F);
        txtSearch.Location = new Point(0, 40);
        txtSearch.Name = "txtSearch";
        txtSearch.PlaceholderText = "🔍  ค้นหาชื่อตาราง...";
        txtSearch.Size = new Size(624, 26);
        txtSearch.TabIndex = 2;
        txtSearch.TextChanged += txtSearch_TextChanged;
        //
        // dgvTables
        //
        dgvTables.AllowUserToAddRows = false;
        dgvTables.AllowUserToDeleteRows = false;
        dgvTables.AllowUserToResizeRows = false;
        alternatingRowStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgvTables.AlternatingRowsDefaultCellStyle = alternatingRowStyle;
        dgvTables.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvTables.BackgroundColor = Color.White;
        dgvTables.BorderStyle = BorderStyle.None;
        dgvTables.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgvTables.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        headerStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        headerStyle.BackColor = Color.FromArgb(241, 245, 249);
        headerStyle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        headerStyle.ForeColor = Color.FromArgb(51, 65, 85);
        headerStyle.Padding = new Padding(8, 0, 0, 0);
        headerStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
        headerStyle.SelectionForeColor = Color.FromArgb(51, 65, 85);
        headerStyle.WrapMode = DataGridViewTriState.False;
        dgvTables.ColumnHeadersDefaultCellStyle = headerStyle;
        dgvTables.ColumnHeadersHeight = 40;
        dgvTables.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvTables.Columns.AddRange(new DataGridViewColumn[] { colNo, colSchema, colTableName });
        cellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        cellStyle.BackColor = Color.White;
        cellStyle.Font = new Font("Segoe UI", 10F);
        cellStyle.ForeColor = Color.FromArgb(30, 41, 59);
        cellStyle.Padding = new Padding(8, 0, 0, 0);
        cellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        cellStyle.SelectionForeColor = Color.FromArgb(30, 58, 138);
        cellStyle.WrapMode = DataGridViewTriState.False;
        dgvTables.DefaultCellStyle = cellStyle;
        dgvTables.Dock = DockStyle.Fill;
        dgvTables.EnableHeadersVisualStyles = false;
        dgvTables.GridColor = Color.FromArgb(226, 232, 240);
        dgvTables.Location = new Point(20, 102);
        dgvTables.MultiSelect = false;
        dgvTables.Name = "dgvTables";
        dgvTables.ReadOnly = true;
        dgvTables.RowHeadersVisible = false;
        dgvTables.RowTemplate.Height = 34;
        dgvTables.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvTables.Size = new Size(624, 418);
        dgvTables.TabIndex = 1;
        //
        // colNo
        //
        colNo.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        numberCellStyle.ForeColor = Color.FromArgb(100, 116, 139);
        colNo.DefaultCellStyle = numberCellStyle;
        colNo.HeaderText = "#";
        colNo.Name = "colNo";
        colNo.ReadOnly = true;
        colNo.Resizable = DataGridViewTriState.False;
        colNo.Width = 70;
        //
        // colSchema
        //
        colSchema.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        colSchema.HeaderText = "Schema";
        colSchema.Name = "colSchema";
        colSchema.ReadOnly = true;
        colSchema.Width = 110;
        //
        // colTableName
        //
        colTableName.HeaderText = "Table Name";
        colTableName.Name = "colTableName";
        colTableName.ReadOnly = true;
        //
        // MainForm
        //
        AcceptButton = btnRetrieve;
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(241, 245, 249);
        ClientSize = new Size(1320, 820);
        Controls.Add(tlpMain);
        Controls.Add(statusStrip);
        Controls.Add(pnlHeader);
        Font = new Font("Segoe UI", 9.75F);
        MinimumSize = new Size(1180, 760);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Import Excel to SQL";
        Load += MainForm_Load;
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        tlpMain.ResumeLayout(false);
        pnlConnection.ResumeLayout(false);
        pnlConnection.PerformLayout();
        pnlRight.ResumeLayout(false);
        pnlTabBar.ResumeLayout(false);
        pnlTables.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvTables).EndInit();
        pnlTablesHeader.ResumeLayout(false);
        pnlTablesHeader.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Panel pnlHeader;
    private Label lblTitle;
    private Label lblSubtitle;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatus;
    private TableLayoutPanel tlpMain;
    private Panel pnlConnection;
    private Label lblConnectionTitle;
    private Label lblClient;
    private ComboBox cboClient;
    private Label lblServer;
    private TextBox txtServer;
    private Label lblDatabase;
    private TextBox txtDatabase;
    private Label lblUser;
    private TextBox txtUser;
    private Label lblPassword;
    private TextBox txtPassword;
    private CheckBox chkShowPassword;
    private Button btnRetrieve;
    private Button btnClear;
    private Panel pnlRight;
    private Panel pnlTabBar;
    private Button btnTabTables;
    private Button btnTabImport;
    private Controls.ImportView importView;
    private Panel pnlTables;
    private Panel pnlTablesHeader;
    private Label lblTablesTitle;
    private Label lblTableCount;
    private TextBox txtSearch;
    private DataGridView dgvTables;
    private DataGridViewTextBoxColumn colNo;
    private DataGridViewTextBoxColumn colSchema;
    private DataGridViewTextBoxColumn colTableName;
}
