using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Dashboard;

public class DashboardControl : UserControl
{
    private readonly int _roleId;
    private readonly ReportService _reportService = new();

    private Label _lblActiveClasses = new();
    private Label _lblTotalStudents = new();
    private Label _lblTeachers = new();
    private Label _lblRevenue = new();
    private DataGridView _dgvAlerts = new();
    private DataGridView _dgvUpcoming = new();

    public DashboardControl(int roleId = 1)
    {
        _roleId = roleId;
        BuildUI();
        if (!DesignModeHelper.IsInDesignMode)
            _ = LoadDataAsync();
    }

    private void BuildUI()
    {
        BackColor = AppTheme.ContentBg;
        Padding = new Padding(20);

        var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.Transparent };
        var lblTitle = new Label
        {
            Text = "Trang chủ",
            Font = AppTheme.FontTitle,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(0, 8),
            AutoSize = true
        };
        var lblSub = new Label
        {
            Text = $"Xin chào, {AppSession.Current?.FullName ?? "Người dùng"}! Đây là tổng quan hệ thống EN-VN Center.",
            Font = AppTheme.FontBody,
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(0, 38),
            AutoSize = true
        };
        pnlTitle.Controls.AddRange(new Control[] { lblTitle, lblSub });

        // Stat cards
        var pnlCards = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.Transparent };
        pnlCards.Padding = new Padding(0, 8, 0, 8);

        _lblActiveClasses = new Label { Text = "—", Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
        _lblTotalStudents = new Label { Text = "—", Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
        _lblTeachers = new Label { Text = "—", Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };
        _lblRevenue = new Label { Text = "—", Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = AppTheme.TextPrimary };

        var cards = new[]
        {
            MakeStatCard("Lớp đang hoạt động", _lblActiveClasses, "Học kỳ này", AppTheme.Info),
            MakeStatCard("Tổng học sinh", _lblTotalStudents, "Trong hệ thống", AppTheme.Success),
            MakeStatCard("Giáo viên", _lblTeachers, "Đang hoạt động", AppTheme.Warning),
            MakeStatCard("Doanh thu tháng", _lblRevenue, "VNĐ", AppTheme.Primary)
        };
        int cx = 0;
        foreach (var card in cards)
        {
            card.Location = new Point(cx, 8);
            pnlCards.Controls.Add(card);
            cx += card.Width + 16;
        }

        // Bottom tables
        var pnlTables = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

        _dgvAlerts = AppTheme.MakeGrid();
        _dgvAlerts.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Lớp học", FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Loại cảnh báo", FillWeight = 25 },
            new DataGridViewTextBoxColumn { HeaderText = "Mô tả", FillWeight = 45 }
        );

        _dgvUpcoming = AppTheme.MakeGrid();
        _dgvUpcoming.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Lớp học", FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Khóa học", FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Khai giảng", FillWeight = 20 },
            new DataGridViewTextBoxColumn { HeaderText = "Sĩ số", FillWeight = 20 }
        );

        var pnlLeft = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        MakeTableCard("Cảnh báo hệ thống", _dgvAlerts, pnlLeft);

        var pnlRight = new Panel { Dock = DockStyle.Right, Width = 450, BackColor = Color.Transparent, Padding = new Padding(8, 0, 0, 0) };
        MakeTableCard("Lớp sắp khai giảng (7 ngày tới)", _dgvUpcoming, pnlRight);

        pnlTables.Controls.Add(pnlLeft);
        pnlTables.Controls.Add(pnlRight);

        Controls.Add(pnlTables);
        Controls.Add(pnlCards);
        Controls.Add(pnlTitle);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var dashboard = await _reportService.GetDashboardAsync();
            if (dashboard == null) return;

            if (InvokeRequired)
            {
                Invoke(() => UpdateUI(dashboard));
            }
            else
            {
                UpdateUI(dashboard);
            }
        }
        catch { }
    }

    private void UpdateUI(Models.DTOs.DashboardDto dashboard)
    {
        _lblActiveClasses.Text = dashboard.ActiveClasses.ToString();
        _lblTotalStudents.Text = dashboard.TotalStudents.ToString();
        _lblTeachers.Text = dashboard.TotalTeachers.ToString();
        _lblRevenue.Text = $"{dashboard.MonthlyRevenue:N0}";

        _dgvAlerts.Rows.Clear();
        foreach (var alert in dashboard.Alerts)
            _dgvAlerts.Rows.Add(alert.ClassName, alert.AlertType == "AbsenceAlert" ? "Vắng nhiều" : alert.AlertType, alert.Message);

        _dgvUpcoming.Rows.Clear();
    }

    private static Panel MakeStatCard(string title, Label valLabel, string sub, Color accent)
    {
        var card = new Panel { Size = new Size(220, 100), BackColor = Color.White };
        var bar = new Panel { Location = new Point(0, 0), Size = new Size(4, 100), BackColor = accent };
        var lblTitle = new Label
        {
            Text = title, Font = AppTheme.FontBodyBold, ForeColor = AppTheme.TextSecondary,
            Location = new Point(16, 54), AutoSize = true
        };
        var lblSub = new Label
        {
            Text = sub, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextMuted,
            Location = new Point(16, 76), AutoSize = true
        };
        valLabel.Location = new Point(16, 14);
        valLabel.AutoSize = true;
        card.Controls.AddRange(new Control[] { bar, valLabel, lblTitle, lblSub });
        return card;
    }

    private static void MakeTableCard(string title, DataGridView grid, Panel parent)
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var hdr = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.White };
        hdr.Paint += (s, e) =>
        {
            using var p = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(p, 0, 43, ((Control)s!).Width, 43);
        };
        var lbl = new Label
        {
            Text = title, Font = AppTheme.FontBodyBold, ForeColor = AppTheme.TextPrimary,
            Location = new Point(16, 0), Size = new Size(380, 44), TextAlign = ContentAlignment.MiddleLeft
        };
        hdr.Controls.Add(lbl);
        card.Controls.Add(grid);
        card.Controls.Add(hdr);
        parent.Controls.Add(card);
    }
}
