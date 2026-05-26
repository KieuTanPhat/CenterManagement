using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;
using CenterManagement.WinForms.Views.Shared;

namespace CenterManagement.WinForms.Views.Reports;

public class RevenueControl : UserControl
{
    private readonly ReportService _service = new();
    private Label _lblRevenue = new(), _lblDeposit = new(), _lblFull = new(), _status = new();
    private DataGridView _grid = new();

    public RevenueControl()
    {
        BackColor = AppTheme.ContentBg;
        Dock = DockStyle.Fill;
        BuildUI();
        if (!DesignModeHelper.IsInDesignMode)
            _ = LoadAsync();
    }

    private void BuildUI()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var head = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        head.Paint += (s, e) => { using var p = new Pen(AppTheme.Border); e.Graphics.DrawLine(p, 0, 59, ((Control)s!).Width, 59); };
        head.Controls.Add(new Label { Text = "💸  Quản lý Doanh thu", Font = AppTheme.FontSubtitle, ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true });

        var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(250, 250, 252) };
        bar.Paint += (s, e) => { using var p = new Pen(AppTheme.Border); e.Graphics.DrawLine(p, 0, 51, ((Control)s!).Width, 51); };
        var lblMonthLbl = new Label { Text = "Tháng:", Location = new Point(16, 16), AutoSize = true, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary };
        var cmbMonth = new ComboBox { Location = new Point(70, 12), Size = new Size(80, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        for (int m = 1; m <= 12; m++) cmbMonth.Items.Add($"T{m}");
        cmbMonth.SelectedIndex = DateTime.Now.Month - 1;
        var lblYear = new Label { Text = "Năm:", Location = new Point(162, 16), AutoSize = true, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary };
        var cmbYear = new ComboBox { Location = new Point(198, 12), Size = new Size(90, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        for (int y = DateTime.Now.Year - 2; y <= DateTime.Now.Year + 1; y++) cmbYear.Items.Add(y.ToString());
        cmbYear.SelectedItem = DateTime.Now.Year.ToString();
        var btn = AppTheme.MakePrimaryBtn("Xem", 70, 28);
        btn.Location = new Point(300, 12);
        btn.Click += async (_, _) => await LoadAsync(cmbMonth.SelectedIndex + 1, int.Parse(cmbYear.SelectedItem?.ToString() ?? "2026"));
        bar.Controls.AddRange(new Control[] { lblMonthLbl, cmbMonth, lblYear, cmbYear, btn });

        var pnlCards = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White };
        _lblRevenue = new Label { Text = "—", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = AppTheme.Success };
        _lblDeposit = new Label { Text = "—", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = AppTheme.Info };
        _lblFull = new Label { Text = "—", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = AppTheme.Warning };
        var cards = new[] {
            MakeCard("Tổng doanh thu", _lblRevenue, AppTheme.Success),
            MakeCard("Thu từ đặt cọc", _lblDeposit, AppTheme.Info),
            MakeCard("Thu còn lại", _lblFull, AppTheme.Warning)
        };
        int cx = 8;
        foreach (var c in cards) { c.Location = new Point(cx, 8); pnlCards.Controls.Add(c); cx += c.Width + 12; }

        _grid = AppTheme.MakeGrid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Học viên", FillWeight = 22 },
            new DataGridViewTextBoxColumn { HeaderText = "Lớp học", FillWeight = 22 },
            new DataGridViewTextBoxColumn { HeaderText = "Loại thanh toán", FillWeight = 18 },
            new DataGridViewTextBoxColumn { HeaderText = "Số tiền", FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Ngày thanh toán", FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", FillWeight = 10 }
        );

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(248, 249, 252) };
        _status = new Label { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary };
        foot.Controls.Add(_status);

        card.Controls.Add(_grid);
        card.Controls.Add(foot);
        card.Controls.Add(pnlCards);
        card.Controls.Add(bar);
        card.Controls.Add(head);
        Controls.Add(card);
    }

    private async Task LoadAsync(int month = 0, int year = 0)
    {
        if (month == 0) month = DateTime.Now.Month;
        if (year == 0) year = DateTime.Now.Year;
        Cursor = Cursors.WaitCursor;
        try
        {
            var report = await _service.GetRevenueAsync(month, year);
            if (report != null)
            {
                _lblRevenue.Text = $"{report.TotalRevenue:N0} đ";
                _lblDeposit.Text = $"{report.DepositRevenue:N0} đ";
                _lblFull.Text = $"{report.RemainingRevenue:N0} đ";
                _grid.Rows.Clear();
                foreach (var r in report.Details)
                    _grid.Rows.Add(r.StudentName, r.ClassName, r.PaymentType,
                        $"{r.Amount:N0} đ", r.PaymentDate.ToString("dd/MM/yyyy"), r.Status.ToString());
                _status.Text = $"Báo cáo tháng {month}/{year} — {report.TotalEnrollments} giao dịch";
            }
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
        finally { Cursor = Cursors.Default; }
    }

    private static Panel MakeCard(string title, Label valLabel, Color accent)
    {
        var card = new Panel { Size = new Size(200, 84), BackColor = Color.White };
        card.Paint += (s, e) => { using var pen = new Pen(AppTheme.Border); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };
        var bar = new Panel { Location = new Point(0, 0), Size = new Size(4, 84), BackColor = accent };
        valLabel.Location = new Point(14, 14);
        valLabel.AutoSize = true;
        var lbl = new Label { Text = title, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary, Location = new Point(14, 52), AutoSize = true };
        card.Controls.AddRange(new Control[] { bar, valLabel, lbl });
        return card;
    }
}
