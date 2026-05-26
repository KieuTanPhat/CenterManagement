#nullable disable

namespace CenterManagement.WinForms.Views.Main;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    // Header
    private Panel      pnlHeader;
    private PictureBox picHeaderLogo;
    private Label      lblAppName, lblUserName, lblUserRole;
    private Button     btnLogout;
    private Panel      pnlUserInfo;

    // Body
    private Panel      pnlBody;
    private Panel      pnlSidebarOuter;
    private Panel      pnlSidebarHeader;
    private Label      lblSidebarTitle;
    private Panel      pnlSidebarItems;   // scrollable
    private Panel      pnlContent;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();

        // ── Form ─────────────────────────────────────────────────────────────
        ClientSize      = new Size(1240, 720);
        MinimumSize     = new Size(1100, 600);
        FormBorderStyle = FormBorderStyle.Sizable;
        Text            = "VN-EN English Center – Hệ thống quản lý";
        StartPosition   = FormStartPosition.CenterScreen;
        Font            = new Font("Segoe UI", 9F);
        BackColor       = Core.AppTheme.ContentBg;

        // ═════════════════════════════════════════════════════════════════════
        // HEADER (60px)
        // ═════════════════════════════════════════════════════════════════════
        pnlHeader           = new Panel();
        pnlHeader.Dock      = DockStyle.Top;
        pnlHeader.Height    = 60;
        pnlHeader.BackColor = Core.AppTheme.Header;

        picHeaderLogo          = new PictureBox();
        picHeaderLogo.Location = new Point(10, 8);
        picHeaderLogo.Size     = new Size(44, 44);
        picHeaderLogo.SizeMode = PictureBoxSizeMode.Zoom;
        picHeaderLogo.BackColor= Color.Transparent;

        lblAppName           = new Label();
        lblAppName.Text      = "VN-EN English Center";
        lblAppName.Font      = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblAppName.ForeColor = Color.White;
        lblAppName.Location  = new Point(62, 10);
        lblAppName.Size      = new Size(320, 26);
        lblAppName.AutoSize  = false;

        var lblAppSub      = new Label();
        lblAppSub.Text     = "Hệ thống quản lý trung tâm tiếng Anh";
        lblAppSub.Font     = new Font("Segoe UI", 8F);
        lblAppSub.ForeColor= Color.FromArgb(255, 200, 200);
        lblAppSub.Location = new Point(63, 36);
        lblAppSub.Size     = new Size(300, 16);

        // User info + logout (bên phải header)
        pnlUserInfo           = new Panel();
        pnlUserInfo.BackColor = Color.Transparent;
        pnlUserInfo.Size      = new Size(280, 60);
        pnlUserInfo.Anchor    = AnchorStyles.Top | AnchorStyles.Right;

        lblUserName           = new Label();
        lblUserName.Text      = "";
        lblUserName.Font      = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblUserName.ForeColor = Color.White;
        lblUserName.Location  = new Point(0, 8);
        lblUserName.Size      = new Size(200, 22);
        lblUserName.TextAlign = ContentAlignment.MiddleRight;

        lblUserRole           = new Label();
        lblUserRole.Text      = "";
        lblUserRole.Font      = new Font("Segoe UI", 8F);
        lblUserRole.ForeColor = Color.FromArgb(255, 200, 200);
        lblUserRole.Location  = new Point(0, 30);
        lblUserRole.Size      = new Size(200, 16);
        lblUserRole.TextAlign = ContentAlignment.MiddleRight;

        btnLogout             = new Button();
        btnLogout.Text        = "⏻  Đăng xuất";
        btnLogout.Font        = new Font("Segoe UI", 8.5F);
        btnLogout.ForeColor   = Color.White;
        btnLogout.BackColor   = Color.FromArgb(160, 5, 25);
        btnLogout.FlatStyle   = FlatStyle.Flat;
        btnLogout.Location    = new Point(208, 15);
        btnLogout.Size        = new Size(108, 30);
        btnLogout.Cursor      = Cursors.Hand;
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.Click      += btnLogout_Click;

        pnlUserInfo.Controls.AddRange(new Control[] { lblUserName, lblUserRole, btnLogout });

        pnlHeader.Controls.AddRange(new Control[]
            { picHeaderLogo, lblAppName, lblAppSub, pnlUserInfo });
        pnlHeader.Resize += pnlHeader_Resize;

        // ═════════════════════════════════════════════════════════════════════
        // BODY = Sidebar + Content
        // ═════════════════════════════════════════════════════════════════════
        pnlBody      = new Panel();
        pnlBody.Dock = DockStyle.Fill;

        // ── Sidebar outer ─────────────────────────────────────────────────────
        pnlSidebarOuter           = new Panel();
        pnlSidebarOuter.Dock      = DockStyle.Left;
        pnlSidebarOuter.Width     = 220;
        pnlSidebarOuter.BackColor = Core.AppTheme.Sidebar;

        // Logo / tên ứng dụng trong sidebar
        pnlSidebarHeader           = new Panel();
        pnlSidebarHeader.Dock      = DockStyle.Top;
        pnlSidebarHeader.Height    = 10;
        pnlSidebarHeader.BackColor = Color.FromArgb(15, 18, 32);

        lblSidebarTitle           = new Label();
        lblSidebarTitle.Text      = "MENU";
        lblSidebarTitle.Font      = new Font("Segoe UI", 7F, FontStyle.Bold);
        lblSidebarTitle.ForeColor = Color.FromArgb(70, 78, 110);
        lblSidebarTitle.Dock      = DockStyle.Fill;
        lblSidebarTitle.TextAlign = ContentAlignment.MiddleCenter;
        pnlSidebarHeader.Controls.Add(lblSidebarTitle);

        // Danh sách nav items (scrollable)
        pnlSidebarItems              = new Panel();
        pnlSidebarItems.AutoScroll   = true;
        pnlSidebarItems.Dock         = DockStyle.Fill;
        pnlSidebarItems.BackColor    = Core.AppTheme.Sidebar;
        pnlSidebarItems.Width        = 220;

        pnlSidebarOuter.Controls.Add(pnlSidebarItems);
        pnlSidebarOuter.Controls.Add(pnlSidebarHeader);

        // ── Content panel ─────────────────────────────────────────────────────
        pnlContent           = new Panel();
        pnlContent.Dock      = DockStyle.Fill;
        pnlContent.BackColor = Core.AppTheme.ContentBg;

        // Đường phân cách giữa sidebar và content
        var splitter      = new Panel();
        splitter.Dock     = DockStyle.Left;
        splitter.Width    = 1;
        splitter.BackColor= Color.FromArgb(10, 14, 28);

        pnlBody.Controls.Add(pnlContent);
        pnlBody.Controls.Add(splitter);
        pnlBody.Controls.Add(pnlSidebarOuter);

        Controls.Add(pnlBody);
        Controls.Add(pnlHeader);

        ResumeLayout(false);
        PerformLayout();
    }
}
