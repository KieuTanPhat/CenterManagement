using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;
using CenterManagement.WinForms.Views.Attendance;
using CenterManagement.WinForms.Views.Branches;
using CenterManagement.WinForms.Views.Classes;
using CenterManagement.WinForms.Views.Courses;
using CenterManagement.WinForms.Views.Dashboard;
using CenterManagement.WinForms.Views.Employees;
using CenterManagement.WinForms.Views.Enrollment;
using CenterManagement.WinForms.Views.Grades;
using CenterManagement.WinForms.Views.Leave;
using CenterManagement.WinForms.Views.Notifications;
using CenterManagement.WinForms.Views.Payments;
using CenterManagement.WinForms.Views.Profile;
using CenterManagement.WinForms.Views.Reports;
using CenterManagement.WinForms.Views.Rooms;
using CenterManagement.WinForms.Views.Schedule;
using CenterManagement.WinForms.Views.Students;
using CenterManagement.WinForms.Views.Teachers;
using CenterManagement.WinForms.Views.Shared;
using CenterManagement.WinForms.Views.Users;

namespace CenterManagement.WinForms.Views.Main;

public partial class MainForm : Form
{
    public event EventHandler? LoggedOut;

    // ─── Cấu hình navigation theo role ───────────────────────────────────────
    // (Icon, Nhãn tiếng Việt, Key, Roles được phép)
    private static readonly (string Icon, string Label, string Key, int[] Roles)[] NavItems =
    {
        // ── Tổng quan ──
        ("⊞", "Tổng quan",           "dashboard",    new[] { 1, 2, 3, 4 }),

        // ── Quản lý (SystemAdmin) ──
        ("🏢", "Chi nhánh",           "branches",     new[] { 1 }),
        ("🔑", "Phân quyền / Users",  "users",        new[] { 1 }),

        // ── Quản lý nhân sự ──
        ("👩‍🏫", "Giáo viên",           "teachers",     new[] { 1, 2 }),
        ("👤", "Nhân viên",            "employees",    new[] { 1, 2 }),

        // ── Học vụ quản lý ──
        ("📚", "Học viên",             "students",     new[] { 1, 2, 3 }),
        ("📖", "Khóa học",             "courses",      new[] { 1, 2 }),
        ("🏫", "Lớp học",              "classes",      new[] { 1, 2, 3 }),
        ("🚪", "Phòng học",            "rooms",        new[] { 1, 2 }),
        ("⏰", "Ca học / Lịch",       "timeslots",    new[] { 1, 2 }),

        // ── Tuyển sinh & tài chính ──
        ("📝", "Đăng ký học",          "enrollment",   new[] { 1, 2, 3 }),
        ("💰", "Thu học phí",          "payments",     new[] { 1, 2, 3 }),
        ("💸", "Doanh thu",            "revenue",      new[] { 1, 2 }),
        ("📊", "Công nợ",              "debt",         new[] { 1, 2, 3 }),
        ("🔄", "Chuyển lớp",           "transfer",     new[] { 1, 2, 3 }),

        // ── Học vụ ──
        ("📅", "Lịch học",             "schedule",     new[] { 1, 2, 3 }),
        ("✅", "Điểm danh",            "attendance",   new[] { 1, 2, 3, 4 }),
        ("🎓", "Điểm số / Kết quả",   "grades",       new[] { 1, 2, 4 }),
        ("📦", "Makeup Session",       "makeup",       new[] { 1, 2, 3, 4 }),
        ("🏖", "Nghỉ phép GV",         "leave",        new[] { 1, 2, 4 }),

        // ── Hỗ trợ ──
        ("👨‍👩‍👧", "Phụ huynh",            "parents",      new[] { 1, 2, 3 }),
        ("🗂", "Tài liệu học tập",     "documents",    new[] { 1, 2, 3, 4 }),
        ("💬", "Phản hồi/Khiếu nại",  "complaints",   new[] { 1, 2, 3 }),
        ("🔔", "Thông báo",            "notifications",new[] { 1, 2, 3 }),

        // ── Báo cáo & hệ thống ──
        ("📈", "Báo cáo",              "reports",      new[] { 1, 2, 3 }),
        ("🗒", "Audit Log",            "auditlog",     new[] { 1, 2 }),

        // ── Giáo viên – chức năng cá nhân ──
        ("📆", "Lịch dạy",             "myschedule",   new[] { 4 }),
        ("🏫", "Lớp tôi phụ trách",   "myclasses",    new[] { 4 }),
        ("👥", "Học viên lớp tôi",    "mystudents",   new[] { 4 }),
        ("📝", "Nhận xét học viên",    "studentnotes", new[] { 4 }),
        ("📈", "Tiến độ học tập",      "progress",     new[] { 4 }),
        ("📋", "Bài tập",              "assignments",  new[] { 4 }),
        ("📃", "Báo cáo lớp",          "classreport",  new[] { 4 }),
        ("💵", "Lương / Ca dạy",       "salary",       new[] { 4 }),

        // ── Nhân viên – cá nhân ──
        ("🏖", "Nghỉ phép cá nhân",   "myleave",      new[] { 3 }),

        // ── Chung ──
        ("👤", "Hồ sơ cá nhân",       "profile",      new[] { 3, 4 }),
    };

    private static readonly Dictionary<string, string> SectionBefore = new()
    {
        ["branches"]     = "QUẢN LÝ HỆ THỐNG",
        ["teachers"]     = "NHÂN SỰ",
        ["students"]     = "HỌC VỤ",
        ["enrollment"]   = "TUYỂN SINH & TÀI CHÍNH",
        ["schedule"]     = "ĐIỂM DANH & LỊCH",
        ["parents"]      = "HỖ TRỢ",
        ["reports"]      = "BÁO CÁO & HỆ THỐNG",
        ["myschedule"]   = "LỊCH & LỚP DẠY",
        ["mystudents"]   = "HỌC VIÊN & GIẢNG DẠY",
        ["assignments"]  = "GIẢNG DẠY",
        ["salary"]       = "CÁ NHÂN",
        ["myleave"]      = "CÁ NHÂN",
        ["profile"]      = "CÁ NHÂN",
    };

    // ─── Trạng thái thu gọn của từng section ────────────────────────────────
    private readonly Dictionary<string, bool> _collapsed = new();

    private Control? _currentContent;
    private Panel? _activeNavPanel;
    private readonly int _roleId;

    public MainForm()
    {
        InitializeComponent();
        _roleId = AppSession.Current?.RoleId ?? 1;
        SetupHeader();
        BuildSidebar();
        Navigate("dashboard");

        // Phím tắt toàn cục
        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F5)
        {
            // Reload content hiện tại
            var tag = _activeNavPanel?.Tag?.ToString();
            if (!string.IsNullOrEmpty(tag)) Navigate(tag);
        }
    }

    private void SetupHeader()
    {
        lblUserName.Text = AppSession.Current?.FullName ?? "Người dùng";
        lblUserRole.Text = GetRoleName(_roleId);
        LoadHeaderLogo();
        pnlHeader_Resize(pnlHeader, EventArgs.Empty);
    }

    private void LoadHeaderLogo()
    {
        try
        {
            var path = Path.Combine(Application.StartupPath, "Resources", "vn_en_logo.png");
            if (File.Exists(path)) picHeaderLogo.Image = Image.FromFile(path);
        }
        catch { }
    }

    private void BuildSidebar()
    {
        pnlSidebarItems.Controls.Clear();
        var y = 8;
        string? currentSection = null;

        foreach (var item in NavItems)
        {
            if (!item.Roles.Contains(_roleId)) continue;

            // Kiểm tra xem item này có bắt đầu section mới không
            if (SectionBefore.TryGetValue(item.Key, out var sectionTitle) && currentSection != sectionTitle)
            {
                currentSection = sectionTitle;
                var isCollapsed = _collapsed.GetValueOrDefault(currentSection, false);
                var header = BuildSectionHeader(currentSection, y, isCollapsed);
                pnlSidebarItems.Controls.Add(header);
                y += 30;
            }

            // Bỏ qua item nếu section đang thu gọn
            if (currentSection != null && _collapsed.GetValueOrDefault(currentSection, false))
                continue;

            var navPanel = BuildNavItem(item.Icon, item.Label, item.Key, y);
            pnlSidebarItems.Controls.Add(navPanel);
            y += 40;
        }

        pnlSidebarItems.Height = y + 8;
    }

    private Panel BuildSectionHeader(string title, int y, bool collapsed)
    {
        var panel = new Panel
        {
            Location = new Point(0, y), Size = new Size(220, 30),
            BackColor = Color.FromArgb(28, 32, 52), Cursor = Cursors.Hand, Tag = $"sec:{title}"
        };
        var lbl = new Label
        {
            Text = title, Font = AppTheme.FontSection, ForeColor = AppTheme.SidebarSection,
            Location = new Point(12, 6), Size = new Size(175, 18), BackColor = Color.Transparent
        };
        var arrow = new Label
        {
            Text = collapsed ? "▶" : "▼", Font = AppTheme.FontSection, ForeColor = AppTheme.SidebarSection,
            Location = new Point(197, 6), Size = new Size(16, 18), BackColor = Color.Transparent, Cursor = Cursors.Hand
        };

        void Toggle(object? s, EventArgs e)
        {
            _collapsed[title] = !_collapsed.GetValueOrDefault(title, false);
            BuildSidebar();
        }

        panel.Click  += Toggle;
        lbl.Click    += Toggle;
        arrow.Click  += Toggle;
        panel.Controls.AddRange(new Control[] { lbl, arrow });
        return panel;
    }

    private Panel BuildNavItem(string icon, string label, string key, int y)
    {
        var panel = new Panel
        {
            Location = new Point(0, y), Size = new Size(220, 40),
            BackColor = AppTheme.Sidebar, Cursor = Cursors.Hand, Tag = key
        };
        var indicator = new Panel { Location = new Point(0, 0), Size = new Size(3, 40), BackColor = Color.Transparent };
        var text = new Label
        {
            Text = $"  {icon}  {label}",
            Font = AppTheme.FontSidebar, ForeColor = AppTheme.SidebarText,
            Location = new Point(0, 0), Size = new Size(220, 40),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent, Cursor = Cursors.Hand, Tag = key
        };

        panel.Controls.Add(indicator);
        panel.Controls.Add(text);

        void SetHover(bool hover)
        {
            if (panel == _activeNavPanel) return;
            panel.BackColor = hover ? AppTheme.SidebarHover : AppTheme.Sidebar;
            text.BackColor = panel.BackColor;
        }
        void Click(object? s, EventArgs a) => Navigate(key);

        panel.MouseEnter += (_, _) => SetHover(true);
        panel.MouseLeave += (_, _) => SetHover(false);
        text.MouseEnter += (_, _) => SetHover(true);
        text.MouseLeave += (_, _) => SetHover(false);
        panel.Click += Click;
        text.Click += Click;
        return panel;
    }

    private void Navigate(string key)
    {
        SetActiveNav(key);
        _currentContent?.Dispose();

        Control next = key switch
        {
            "dashboard"     => new DashboardControl(_roleId),
            // SystemAdmin
            "branches"      => new BranchControl(),
            "users"         => new UserManageControl(_roleId),
            // Nhân sự
            "teachers"      => new TeacherManageControl(),
            "employees"     => new EmployeeControl(),
            // Học vụ
            "students"      => new StudentControl(),
            "courses"       => new CourseControl(),
            "classes"       => new ClassControl(_roleId),
            "rooms"         => new RoomControl(),
            "timeslots"     => new TimeSlotControl(),
            // Tuyển sinh & tài chính
            "enrollment"    => new EnrollmentControl(),
            "payments"      => new PaymentControl(),
            "revenue"       => new RevenueControl(),
            "debt"          => new DebtControl(),
            "transfer"      => new TransferControl(),
            // Học vụ
            "schedule"      => new ScheduleControl(_roleId),
            "attendance"    => new AttendanceControl(_roleId),
            "grades"        => new GradeControl(_roleId),
            "makeup"        => new MakeupControl(_roleId),
            "leave"         => new LeaveRequestControl(_roleId),
            // Hỗ trợ
            "parents"       => new ParentControl(),
            "documents"     => new DocumentControl(),
            "complaints"    => new ComplaintControl(),
            "notifications" => new NotificationControl(),
            // Báo cáo
            "reports"       => new ReportControl(),
            "auditlog"      => new AuditLogControl(),
            // Giáo viên cá nhân
            "myschedule"    => new ScheduleControl(_roleId),
            "myclasses"     => new ClassControl(_roleId),
            "mystudents"    => new StudentControl(),
            "studentnotes"  => new StudentNotesControl(),
            "progress"      => new ProgressControl(),
            "assignments"   => new AssignmentControl(),
            "classreport"   => new ReportControl(),
            "salary"        => new SalaryControl(),
            // Nhân viên cá nhân
            "myleave"       => new LeaveRequestControl(_roleId),
            "profile"       => new ProfileControl(),
            _ => new DashboardControl(_roleId)
        };

        next.Dock = DockStyle.Fill;
        _currentContent = next;
        pnlContent.Controls.Clear();
        pnlContent.Controls.Add(next);
    }

    private void SetActiveNav(string key)
    {
        if (_activeNavPanel != null)
        {
            _activeNavPanel.BackColor = AppTheme.Sidebar;
            if (_activeNavPanel.Controls[0] is Panel oldIndicator) oldIndicator.BackColor = Color.Transparent;
            if (_activeNavPanel.Controls[1] is Label oldLabel)
            {
                oldLabel.BackColor = AppTheme.Sidebar;
                oldLabel.ForeColor = AppTheme.SidebarText;
            }
        }

        _activeNavPanel = pnlSidebarItems.Controls.OfType<Panel>()
            .FirstOrDefault(p => p.Tag?.ToString() == key);

        if (_activeNavPanel == null) return;
        _activeNavPanel.BackColor = AppTheme.SidebarActive;
        if (_activeNavPanel.Controls[0] is Panel indicator) indicator.BackColor = Color.White;
        if (_activeNavPanel.Controls[1] is Label label)
        {
            label.BackColor = AppTheme.SidebarActive;
            label.ForeColor = Color.White;
        }
    }

    private async void btnLogout_Click(object? sender, EventArgs e)
    {
        try { await new AuthService().LogoutAsync(); }
        catch
        {
            ApiClient.Instance.ClearAuthToken();
            AppSession.Current?.Clear();
        }
        LoggedOut?.Invoke(this, EventArgs.Empty);
        Close();
    }

    private void pnlHeader_Resize(object? sender, EventArgs e)
    {
        pnlUserInfo.Location = new Point(pnlHeader.Width - pnlUserInfo.Width - 12, 0);
    }

    private static string GetRoleName(int roleId) => roleId switch
    {
        1 => "Quản trị hệ thống",
        2 => "Quản lý chi nhánh",
        3 => "Nhân viên",
        4 => "Giáo viên",
        _ => "Người dùng"
    };
}
