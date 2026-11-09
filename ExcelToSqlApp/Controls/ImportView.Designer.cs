namespace ExcelToSqlApp.Controls;

partial class ImportView
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

    #region Component Designer generated code

    private void InitializeComponent()
    {
        tlpImport = new TableLayoutPanel();
        pnlSource = new Panel();
        cboDuplicateMode = new ComboBox();
        lblDuplicateMode = new Label();
        cboTargetTable = new ComboBox();
        lblTargetTable = new Label();
        cboSheet = new ComboBox();
        lblSheet = new Label();
        lblFileName = new Label();
        btnBrowse = new Button();
        lblImportTitle = new Label();
        splitData = new SplitContainer();
        dgvPreview = new DataGridView();
        pnlPreviewHeader = new Panel();
        lblPreviewInfo = new Label();
        lblPreviewTitle = new Label();
        dgvMapping = new DataGridView();
        colMapColumn = new DataGridViewTextBoxColumn();
        colMapType = new DataGridViewTextBoxColumn();
        colMapAttributes = new DataGridViewTextBoxColumn();
        colMapExcel = new DataGridViewComboBoxColumn();
        colMapConstant = new DataGridViewTextBoxColumn();
        colMapKey = new DataGridViewCheckBoxColumn();
        pnlMappingHeader = new Panel();
        lblMappingInfo = new Label();
        lblMappingTitle = new Label();
        pnlFooter = new Panel();
        btnImport = new Button();
        progressImport = new ProgressBar();
        lblImportStatus = new Label();
        tlpImport.SuspendLayout();
        pnlSource.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitData).BeginInit();
        splitData.Panel1.SuspendLayout();
        splitData.Panel2.SuspendLayout();
        splitData.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvPreview).BeginInit();
        pnlPreviewHeader.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMapping).BeginInit();
        pnlMappingHeader.SuspendLayout();
        pnlFooter.SuspendLayout();
        SuspendLayout();
        //
        // tlpImport
        //
        tlpImport.ColumnCount = 1;
        tlpImport.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpImport.Controls.Add(pnlSource, 0, 0);
        tlpImport.Controls.Add(splitData, 0, 1);
        tlpImport.Controls.Add(pnlFooter, 0, 2);
        tlpImport.Dock = DockStyle.Fill;
        tlpImport.Location = new Point(20, 18);
        tlpImport.Name = "tlpImport";
        tlpImport.RowCount = 3;
        tlpImport.RowStyles.Add(new RowStyle(SizeType.Absolute, 122F));
        tlpImport.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpImport.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
        tlpImport.Size = new Size(860, 584);
        tlpImport.TabIndex = 0;
        //
        // pnlSource
        //
        pnlSource.Controls.Add(cboDuplicateMode);
        pnlSource.Controls.Add(lblDuplicateMode);
        pnlSource.Controls.Add(cboTargetTable);
        pnlSource.Controls.Add(lblTargetTable);
        pnlSource.Controls.Add(cboSheet);
        pnlSource.Controls.Add(lblSheet);
        pnlSource.Controls.Add(lblFileName);
        pnlSource.Controls.Add(btnBrowse);
        pnlSource.Controls.Add(lblImportTitle);
        pnlSource.Dock = DockStyle.Fill;
        pnlSource.Location = new Point(0, 0);
        pnlSource.Margin = new Padding(0);
        pnlSource.Name = "pnlSource";
        pnlSource.Size = new Size(860, 122);
        pnlSource.TabIndex = 0;
        //
        // lblImportTitle
        //
        lblImportTitle.AutoSize = true;
        lblImportTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
        lblImportTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblImportTitle.Location = new Point(0, 0);
        lblImportTitle.Name = "lblImportTitle";
        lblImportTitle.Size = new Size(160, 21);
        lblImportTitle.TabIndex = 0;
        lblImportTitle.Text = "นำเข้าข้อมูลจากไฟล์ Excel";
        //
        // btnBrowse
        //
        btnBrowse.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        btnBrowse.Location = new Point(0, 34);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(150, 36);
        btnBrowse.TabIndex = 1;
        btnBrowse.Text = "📂  Browse Excel";
        btnBrowse.Click += btnBrowse_Click;
        //
        // lblFileName
        //
        lblFileName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblFileName.AutoEllipsis = true;
        lblFileName.BackColor = Color.FromArgb(248, 250, 252);
        lblFileName.ForeColor = Color.FromArgb(148, 163, 184);
        lblFileName.Location = new Point(160, 34);
        lblFileName.Name = "lblFileName";
        lblFileName.Padding = new Padding(10, 0, 10, 0);
        lblFileName.Size = new Size(700, 36);
        lblFileName.TabIndex = 2;
        lblFileName.Text = "ยังไม่ได้เลือกไฟล์ (.xlsx / .xls)";
        lblFileName.TextAlign = ContentAlignment.MiddleLeft;
        //
        // lblSheet
        //
        lblSheet.AutoSize = true;
        lblSheet.ForeColor = Color.FromArgb(71, 85, 105);
        lblSheet.Location = new Point(0, 88);
        lblSheet.Name = "lblSheet";
        lblSheet.Size = new Size(42, 19);
        lblSheet.TabIndex = 3;
        lblSheet.Text = "Sheet";
        //
        // cboSheet
        //
        cboSheet.DropDownStyle = ComboBoxStyle.DropDownList;
        cboSheet.FlatStyle = FlatStyle.Flat;
        cboSheet.Font = new Font("Segoe UI", 10F);
        cboSheet.FormattingEnabled = true;
        cboSheet.Location = new Point(50, 84);
        cboSheet.Name = "cboSheet";
        cboSheet.Size = new Size(180, 25);
        cboSheet.TabIndex = 4;
        cboSheet.SelectedIndexChanged += cboSheet_SelectedIndexChanged;
        //
        // lblTargetTable
        //
        lblTargetTable.AutoSize = true;
        lblTargetTable.ForeColor = Color.FromArgb(71, 85, 105);
        lblTargetTable.Location = new Point(248, 88);
        lblTargetTable.Name = "lblTargetTable";
        lblTargetTable.Size = new Size(92, 19);
        lblTargetTable.TabIndex = 5;
        lblTargetTable.Text = "ตารางปลายทาง";
        //
        // cboTargetTable
        //
        cboTargetTable.DropDownStyle = ComboBoxStyle.DropDownList;
        cboTargetTable.FlatStyle = FlatStyle.Flat;
        cboTargetTable.Font = new Font("Segoe UI", 10F);
        cboTargetTable.FormattingEnabled = true;
        cboTargetTable.Location = new Point(348, 84);
        cboTargetTable.MaxDropDownItems = 20;
        cboTargetTable.Name = "cboTargetTable";
        cboTargetTable.Size = new Size(230, 25);
        cboTargetTable.TabIndex = 6;
        cboTargetTable.SelectionChangeCommitted += cboTargetTable_SelectionChangeCommitted;
        //
        // lblDuplicateMode
        //
        lblDuplicateMode.AutoSize = true;
        lblDuplicateMode.ForeColor = Color.FromArgb(71, 85, 105);
        lblDuplicateMode.Location = new Point(596, 88);
        lblDuplicateMode.Name = "lblDuplicateMode";
        lblDuplicateMode.Size = new Size(80, 19);
        lblDuplicateMode.TabIndex = 7;
        lblDuplicateMode.Text = "ถ้าพบข้อมูลซ้ำ";
        //
        // cboDuplicateMode
        //
        cboDuplicateMode.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDuplicateMode.FlatStyle = FlatStyle.Flat;
        cboDuplicateMode.Font = new Font("Segoe UI", 10F);
        cboDuplicateMode.FormattingEnabled = true;
        cboDuplicateMode.Location = new Point(690, 84);
        cboDuplicateMode.Name = "cboDuplicateMode";
        cboDuplicateMode.Size = new Size(170, 25);
        cboDuplicateMode.TabIndex = 8;
        //
        // splitData
        //
        splitData.Dock = DockStyle.Fill;
        splitData.Location = new Point(0, 122);
        splitData.Margin = new Padding(0);
        splitData.Name = "splitData";
        splitData.Orientation = Orientation.Horizontal;
        splitData.Panel1.Controls.Add(dgvPreview);
        splitData.Panel1.Controls.Add(pnlPreviewHeader);
        splitData.Panel2.Controls.Add(dgvMapping);
        splitData.Panel2.Controls.Add(pnlMappingHeader);
        splitData.Size = new Size(860, 396);
        splitData.SplitterDistance = 180;
        splitData.SplitterWidth = 10;
        splitData.TabIndex = 1;
        //
        // pnlPreviewHeader
        //
        pnlPreviewHeader.Controls.Add(lblPreviewInfo);
        pnlPreviewHeader.Controls.Add(lblPreviewTitle);
        pnlPreviewHeader.Dock = DockStyle.Top;
        pnlPreviewHeader.Location = new Point(0, 0);
        pnlPreviewHeader.Name = "pnlPreviewHeader";
        pnlPreviewHeader.Size = new Size(860, 30);
        pnlPreviewHeader.TabIndex = 0;
        //
        // lblPreviewTitle
        //
        lblPreviewTitle.AutoSize = true;
        lblPreviewTitle.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        lblPreviewTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblPreviewTitle.Location = new Point(0, 4);
        lblPreviewTitle.Name = "lblPreviewTitle";
        lblPreviewTitle.Size = new Size(150, 19);
        lblPreviewTitle.TabIndex = 0;
        lblPreviewTitle.Text = "ตัวอย่างข้อมูล (Preview)";
        //
        // lblPreviewInfo
        //
        lblPreviewInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblPreviewInfo.ForeColor = Color.FromArgb(100, 116, 139);
        lblPreviewInfo.Location = new Point(460, 4);
        lblPreviewInfo.Name = "lblPreviewInfo";
        lblPreviewInfo.Size = new Size(400, 19);
        lblPreviewInfo.TabIndex = 1;
        lblPreviewInfo.TextAlign = ContentAlignment.MiddleRight;
        //
        // dgvPreview
        //
        dgvPreview.Dock = DockStyle.Fill;
        dgvPreview.Location = new Point(0, 30);
        dgvPreview.Name = "dgvPreview";
        dgvPreview.ReadOnly = true;
        dgvPreview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvPreview.Size = new Size(860, 150);
        dgvPreview.TabIndex = 1;
        dgvPreview.DataBindingComplete += dgvPreview_DataBindingComplete;
        //
        // pnlMappingHeader
        //
        pnlMappingHeader.Controls.Add(lblMappingInfo);
        pnlMappingHeader.Controls.Add(lblMappingTitle);
        pnlMappingHeader.Dock = DockStyle.Top;
        pnlMappingHeader.Location = new Point(0, 0);
        pnlMappingHeader.Name = "pnlMappingHeader";
        pnlMappingHeader.Size = new Size(860, 30);
        pnlMappingHeader.TabIndex = 0;
        //
        // lblMappingTitle
        //
        lblMappingTitle.AutoSize = true;
        lblMappingTitle.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        lblMappingTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblMappingTitle.Location = new Point(0, 4);
        lblMappingTitle.Name = "lblMappingTitle";
        lblMappingTitle.Size = new Size(210, 19);
        lblMappingTitle.TabIndex = 0;
        lblMappingTitle.Text = "จับคู่คอลัมน์ (Column Mapping)";
        //
        // lblMappingInfo
        //
        lblMappingInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblMappingInfo.ForeColor = Color.FromArgb(100, 116, 139);
        lblMappingInfo.Location = new Point(360, 4);
        lblMappingInfo.Name = "lblMappingInfo";
        lblMappingInfo.Size = new Size(500, 19);
        lblMappingInfo.TabIndex = 1;
        lblMappingInfo.TextAlign = ContentAlignment.MiddleRight;
        //
        // dgvMapping
        //
        dgvMapping.Columns.AddRange(new DataGridViewColumn[] { colMapColumn, colMapType, colMapAttributes, colMapExcel, colMapConstant, colMapKey });
        dgvMapping.Dock = DockStyle.Fill;
        dgvMapping.EditMode = DataGridViewEditMode.EditOnEnter;
        dgvMapping.Location = new Point(0, 30);
        dgvMapping.MultiSelect = false;
        dgvMapping.Name = "dgvMapping";
        dgvMapping.Size = new Size(860, 176);
        dgvMapping.TabIndex = 1;
        dgvMapping.CellValueChanged += dgvMapping_CellValueChanged;
        dgvMapping.CurrentCellDirtyStateChanged += dgvMapping_CurrentCellDirtyStateChanged;
        dgvMapping.DataError += dgvMapping_DataError;
        //
        // colMapColumn
        //
        colMapColumn.HeaderText = "คอลัมน์ SQL";
        colMapColumn.Name = "colMapColumn";
        colMapColumn.ReadOnly = true;
        colMapColumn.Width = 150;
        //
        // colMapType
        //
        colMapType.HeaderText = "ชนิดข้อมูล";
        colMapType.Name = "colMapType";
        colMapType.ReadOnly = true;
        colMapType.Width = 120;
        //
        // colMapAttributes
        //
        colMapAttributes.HeaderText = "คุณสมบัติ";
        colMapAttributes.Name = "colMapAttributes";
        colMapAttributes.ReadOnly = true;
        colMapAttributes.Width = 170;
        //
        // colMapExcel
        //
        colMapExcel.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colMapExcel.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
        colMapExcel.FlatStyle = FlatStyle.Flat;
        colMapExcel.HeaderText = "คอลัมน์ Excel";
        colMapExcel.MinimumWidth = 180;
        colMapExcel.Name = "colMapExcel";
        //
        // colMapConstant
        //
        colMapConstant.HeaderText = "ค่าคงที่ (ถ้าไม่มีใน Excel)";
        colMapConstant.Name = "colMapConstant";
        colMapConstant.Width = 160;
        //
        // colMapKey
        //
        colMapKey.HeaderText = "คีย์ตรวจซ้ำ";
        colMapKey.Name = "colMapKey";
        colMapKey.Width = 90;
        //
        // pnlFooter
        //
        pnlFooter.Controls.Add(btnImport);
        pnlFooter.Controls.Add(progressImport);
        pnlFooter.Controls.Add(lblImportStatus);
        pnlFooter.Dock = DockStyle.Fill;
        pnlFooter.Location = new Point(0, 518);
        pnlFooter.Margin = new Padding(0);
        pnlFooter.Name = "pnlFooter";
        pnlFooter.Size = new Size(860, 66);
        pnlFooter.TabIndex = 2;
        //
        // lblImportStatus
        //
        lblImportStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblImportStatus.AutoEllipsis = true;
        lblImportStatus.ForeColor = Color.FromArgb(71, 85, 105);
        lblImportStatus.Location = new Point(0, 14);
        lblImportStatus.Name = "lblImportStatus";
        lblImportStatus.Size = new Size(650, 19);
        lblImportStatus.TabIndex = 0;
        lblImportStatus.Text = "เลือกไฟล์ Excel เพื่อเริ่มต้น";
        //
        // progressImport
        //
        progressImport.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        progressImport.Location = new Point(0, 40);
        progressImport.Name = "progressImport";
        progressImport.Size = new Size(650, 14);
        progressImport.Style = ProgressBarStyle.Continuous;
        progressImport.TabIndex = 1;
        //
        // btnImport
        //
        btnImport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnImport.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        btnImport.Location = new Point(670, 14);
        btnImport.Name = "btnImport";
        btnImport.Size = new Size(190, 42);
        btnImport.TabIndex = 2;
        btnImport.Text = "⬆  Import ข้อมูล";
        btnImport.Click += btnImport_Click;
        //
        // ImportView
        //
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        Controls.Add(tlpImport);
        Font = new Font("Segoe UI", 9.75F);
        Name = "ImportView";
        Padding = new Padding(20, 18, 20, 20);
        Size = new Size(900, 622);
        tlpImport.ResumeLayout(false);
        pnlSource.ResumeLayout(false);
        pnlSource.PerformLayout();
        splitData.Panel1.ResumeLayout(false);
        splitData.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitData).EndInit();
        splitData.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvPreview).EndInit();
        pnlPreviewHeader.ResumeLayout(false);
        pnlPreviewHeader.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMapping).EndInit();
        pnlMappingHeader.ResumeLayout(false);
        pnlMappingHeader.PerformLayout();
        pnlFooter.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel tlpImport;
    private Panel pnlSource;
    private Label lblImportTitle;
    private Button btnBrowse;
    private Label lblFileName;
    private Label lblSheet;
    private ComboBox cboSheet;
    private Label lblTargetTable;
    private ComboBox cboTargetTable;
    private Label lblDuplicateMode;
    private ComboBox cboDuplicateMode;
    private SplitContainer splitData;
    private Panel pnlPreviewHeader;
    private Label lblPreviewTitle;
    private Label lblPreviewInfo;
    private DataGridView dgvPreview;
    private Panel pnlMappingHeader;
    private Label lblMappingTitle;
    private Label lblMappingInfo;
    private DataGridView dgvMapping;
    private DataGridViewTextBoxColumn colMapColumn;
    private DataGridViewTextBoxColumn colMapType;
    private DataGridViewTextBoxColumn colMapAttributes;
    private DataGridViewComboBoxColumn colMapExcel;
    private DataGridViewTextBoxColumn colMapConstant;
    private DataGridViewCheckBoxColumn colMapKey;
    private Panel pnlFooter;
    private Label lblImportStatus;
    private ProgressBar progressImport;
    private Button btnImport;
}
