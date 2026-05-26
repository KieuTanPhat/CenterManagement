namespace CenterManagement.WinForms.Core;

internal static class AppTheme
{
    // Brand
    public static readonly Color Primary     = Color.FromArgb(200, 16, 46);
    public static readonly Color PrimaryDark = Color.FromArgb(140, 10, 30);
    public static readonly Color PrimaryLight= Color.FromArgb(220, 55, 82);

    // Sidebar
    public static readonly Color Sidebar          = Color.FromArgb(22, 26, 44);
    public static readonly Color SidebarHover     = Color.FromArgb(38, 43, 68);
    public static readonly Color SidebarActive    = Color.FromArgb(200, 16, 46);
    public static readonly Color SidebarText      = Color.FromArgb(175, 180, 200);
    public static readonly Color SidebarSection   = Color.FromArgb(90, 96, 130);

    // Content
    public static readonly Color ContentBg = Color.FromArgb(242, 244, 250);
    public static readonly Color CardBg    = Color.White;
    public static readonly Color Border    = Color.FromArgb(220, 222, 230);

    // Text
    public static readonly Color TextPrimary   = Color.FromArgb(30, 34, 50);
    public static readonly Color TextSecondary = Color.FromArgb(105, 110, 135);
    public static readonly Color TextMuted     = Color.FromArgb(148, 152, 170);

    // Status
    public static readonly Color Success = Color.FromArgb(34, 153, 84);
    public static readonly Color Warning = Color.FromArgb(230, 126, 34);
    public static readonly Color Danger  = Color.FromArgb(192, 57, 43);
    public static readonly Color Info    = Color.FromArgb(41, 128, 185);

    // Header
    public static readonly Color Header     = Color.FromArgb(200, 16, 46);
    public static readonly Color HeaderText = Color.White;

    // Fonts
    public static Font FontTitle    => new Font("Segoe UI", 14F, FontStyle.Bold);
    public static Font FontSubtitle => new Font("Segoe UI", 11F, FontStyle.Bold);
    public static Font FontBody     => new Font("Segoe UI", 9.5F);
    public static Font FontBodyBold => new Font("Segoe UI", 9.5F, FontStyle.Bold);
    public static Font FontSmall    => new Font("Segoe UI", 8.5F);
    public static Font FontSidebar  => new Font("Segoe UI", 9.5F);
    public static Font FontSection  => new Font("Segoe UI", 7.5F, FontStyle.Bold);

    public static Button MakePrimaryBtn(string text, int w = 120, int h = 36)
    {
        var b = new Button
        {
            Text      = text,
            Size      = new Size(w, h),
            FlatStyle = FlatStyle.Flat,
            BackColor = Primary,
            ForeColor = Color.White,
            Font      = FontBodyBold,
            Cursor    = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    public static Button MakeOutlineBtn(string text, int w = 110, int h = 36)
    {
        var b = new Button
        {
            Text      = text,
            Size      = new Size(w, h),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Primary,
            Font      = FontBodyBold,
            Cursor    = Cursors.Hand
        };
        b.FlatAppearance.BorderColor = Primary;
        b.FlatAppearance.BorderSize  = 1;
        return b;
    }

    public static Button MakeDangerBtn(string text, int w = 100, int h = 36)
    {
        var b = new Button
        {
            Text      = text,
            Size      = new Size(w, h),
            FlatStyle = FlatStyle.Flat,
            BackColor = Danger,
            ForeColor = Color.White,
            Font      = FontBodyBold,
            Cursor    = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    public static DataGridView MakeGrid()
    {
        var g = new DataGridView
        {
            AutoSizeColumnsMode         = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible           = false,
            AllowUserToAddRows          = false,
            ReadOnly                    = true,
            SelectionMode               = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor             = Color.White,
            BorderStyle                 = BorderStyle.None,
            CellBorderStyle             = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle    = DataGridViewHeaderBorderStyle.None,
            EnableHeadersVisualStyles   = false,
            GridColor                   = Color.FromArgb(235, 237, 245),
            Font                        = FontBody,
            MultiSelect                 = false,
            Dock                        = DockStyle.Fill
        };
        g.RowTemplate.Height = 38;
        g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor            = Color.FromArgb(248, 249, 252),
            ForeColor            = TextPrimary,
            Font                 = FontBodyBold,
            Padding              = new Padding(8, 0, 0, 0),
            SelectionBackColor   = Color.FromArgb(248, 249, 252),
            SelectionForeColor   = TextPrimary,
            Alignment            = DataGridViewContentAlignment.MiddleLeft
        };
        g.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor          = Color.White,
            ForeColor          = TextPrimary,
            SelectionBackColor = Color.FromArgb(255, 235, 240),
            SelectionForeColor = PrimaryDark,
            Padding            = new Padding(8, 0, 0, 0)
        };
        g.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor          = Color.FromArgb(251, 251, 253),
            SelectionBackColor = Color.FromArgb(255, 235, 240),
            SelectionForeColor = PrimaryDark
        };
        return g;
    }

    // Tạo badge label cho trạng thái
    public static Label MakeBadge(string text, Color bg)
    {
        return new Label
        {
            Text      = text,
            AutoSize  = true,
            BackColor = bg,
            ForeColor = Color.White,
            Font      = FontSmall,
            Padding   = new Padding(6, 2, 6, 2)
        };
    }
}
