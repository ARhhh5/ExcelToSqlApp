namespace ExcelToSqlApp;

/// <summary>Shared colours and control styling so every screen looks consistent.</summary>
internal static class UiTheme
{
    public static readonly Color Accent = Color.FromArgb(37, 99, 235);
    public static readonly Color AccentHover = Color.FromArgb(29, 78, 216);
    public static readonly Color Success = Color.FromArgb(22, 163, 74);
    public static readonly Color SuccessHover = Color.FromArgb(21, 128, 61);
    public static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
    public static readonly Color TextSecondary = Color.FromArgb(71, 85, 105);
    public static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
    public static readonly Color Border = Color.FromArgb(226, 232, 240);
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceAlt = Color.FromArgb(248, 250, 252);
    public static readonly Color HeaderBack = Color.FromArgb(241, 245, 249);
    public static readonly Color Warning = Color.FromArgb(254, 242, 242);
    public static readonly Color WarningText = Color.FromArgb(185, 28, 28);

    public static void StyleGrid(DataGridView grid)
    {
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeight = 38;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.EnableHeadersVisualStyles = false;
        grid.GridColor = Border;
        grid.RowHeadersVisible = false;
        grid.RowTemplate.Height = 32;

        grid.ColumnHeadersDefaultCellStyle.BackColor = HeaderBack;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderBack;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(51, 65, 85);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
        grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.75F);
        grid.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 58, 138);
        grid.AlternatingRowsDefaultCellStyle.BackColor = SurfaceAlt;
    }

    public static void StylePrimaryButton(Button button, Color back, Color hover)
    {
        button.BackColor = back;
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = hover;
        button.FlatAppearance.MouseDownBackColor = hover;
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
    }

    public static void StyleSecondaryButton(Button button)
    {
        button.BackColor = Surface;
        button.ForeColor = Color.FromArgb(51, 65, 85);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        button.FlatAppearance.MouseOverBackColor = HeaderBack;
        button.FlatAppearance.MouseDownBackColor = Border;
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
    }

    public static void PaintBorder(object? sender, PaintEventArgs e)
    {
        if (sender is not Control control)
        {
            return;
        }

        using var pen = new Pen(Border);
        e.Graphics.DrawRectangle(pen, 0, 0, control.Width - 1, control.Height - 1);
    }
}
