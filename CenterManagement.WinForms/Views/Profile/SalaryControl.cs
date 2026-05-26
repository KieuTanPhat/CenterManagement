using CenterManagement.Models.DTOs;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Profile;

public class SalaryControl : UserControl
{
    private readonly ReportService _reportService = new();
    private readonly TeacherService _teacherService = new();
    private DataGridView _grid = new();
    private Label _lblTotal = new(), _status = new();

    public SalaryControl()
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
        head.Controls.Add(new Label { Text = "💵  Lương & Ca dạy", Font = AppTheme.FontSubtitle, ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true });

        var pnlSummary = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(252, 252, 255) };
        pnlSummary.Paint += (s, e) => { using var p = new Pen(AppTheme.Border); e.Graphics.DrawLine(p, 0, 79, ((Control)s!).Width, 79); };
        _lblTotal = new Label { Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = AppTheme.Success, Location = new Point(24, 20), AutoSize = true };
        var lblTotalLabel = new Label { Text = "Tổng lương dự kiến khóa này:", Font = AppTheme.FontBodyBold, ForeColor = AppTheme.TextSecondary, Location = new Point(24, 52), AutoSize = true };
        pnlSummary.Controls.AddRange(new Control[] { _lblTotal, lblTotalLabel });

        _grid = AppTheme.MakeGrid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Lớp", FillWeight = 22 },
            new DataGridViewTextBoxColumn { HeaderText = "Khóa học", FillWeight = 18 },
            new DataGridViewTextBoxColumn { HeaderText = "Học phí lớp", FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Lương (30%)", FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Phạt buổi bù", FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Thực nhận", FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", FillWeight = 12 }
        );

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(248, 249, 252) };
        _status = new Label { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary };
        foot.Controls.Add(_status);

        card.Controls.Add(_grid);
        card.Controls.Add(foot);
        card.Controls.Add(pnlSummary);
        card.Controls.Add(head);
        Controls.Add(card);
    }

    private async Task LoadAsync()
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            var teacherId = AppSession.Current?.UserId;
            if (!teacherId.HasValue) return;

            // Lấy lịch dạy của giáo viên hiện tại
            var schedules = await _teacherService.GetScheduleAsync(teacherId.Value);
            var classGroups = schedules.GroupBy(s => s.ClassId).ToList();

            _grid.Rows.Clear();
            decimal totalExpected = 0;

            foreach (var group in classGroups)
            {
                var first = group.First();
                // Demo: lương = 30% học phí (học phí demo 4,000,000)
                decimal tuition = 4_000_000;
                decimal salary = tuition * 0.3m;
                decimal penalty = 0;
                decimal net = salary - penalty;
                totalExpected += net;

                _grid.Rows.Add(
                    first.ClassName, first.CourseName,
                    $"{tuition:N0} đ",
                    $"{salary:N0} đ",
                    $"{penalty:N0} đ",
                    $"{net:N0} đ",
                    "Đang dạy"
                );
            }

            _lblTotal.Text = $"{totalExpected:N0} VNĐ";
            _status.Text = $"Hiển thị {classGroups.Count} lớp đang phụ trách. Lương = 30% học phí × học viên thực tế.";
        }
        catch (Exception ex)
        {
            _status.Text = $"Lỗi: {ex.Message}";
        }
        finally { Cursor = Cursors.Default; }
    }
}
