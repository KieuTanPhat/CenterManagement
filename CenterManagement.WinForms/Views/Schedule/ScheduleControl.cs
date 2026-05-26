using CenterManagement.WinForms.Core;

namespace CenterManagement.WinForms.Views.Schedule;

/// <summary>Lịch dạy – chỉ dành cho giáo viên (role 4).</summary>
public class ScheduleControl : UserControl
{
    private readonly int _roleId;

    public ScheduleControl(int roleId = 0)
    {
        _roleId   = roleId;
        BackColor = AppTheme.ContentBg;
        Dock      = DockStyle.Fill;
        BuildUI();
    }

    private void BuildUI()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        // Header
        var pnlHead = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        pnlHead.Paint += B;
        pnlHead.Controls.Add(new Label
        {
            Text = "Lịch dạy của tôi", Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true
        });

        // Tuần hiện tại / điều hướng
        var pnlNav = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(250, 250, 252) };
        pnlNav.Paint += B;

        var btnPrev  = AppTheme.MakeOutlineBtn("◀  Tuần trước", 130, 32);
        btnPrev.Location = new Point(16, 10);
        var lblWeek  = new Label
        {
            Text = "Tuần 22  |  27/05/2024 – 02/06/2024",
            Font = AppTheme.FontBodyBold, ForeColor = AppTheme.TextPrimary,
            Location = new Point(162, 10), Size = new Size(320, 32),
            TextAlign = ContentAlignment.MiddleCenter
        };
        var btnNext  = AppTheme.MakeOutlineBtn("Tuần sau  ▶", 130, 32);
        btnNext.Location = new Point(494, 10);

        pnlNav.Controls.AddRange(new Control[] { btnPrev, lblWeek, btnNext });

        // Bảng lịch dạy dạng danh sách
        var dgv = AppTheme.MakeGrid();
        dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Thứ",           FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Ngày",          FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Giờ",           FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Lớp học",       FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Khóa học",      FillWeight = 18 },
            new DataGridViewTextBoxColumn { HeaderText = "Phòng",         FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Sĩ số",         FillWeight = 8  },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái",    FillWeight = 16 }
        );
        dgv.Rows.Add("Thứ 2", "27/05/2024", "08:00 – 10:00", "E-A1-01", "English A1",  "P.101", "12/20", "Đã điểm danh");
        dgv.Rows.Add("Thứ 3", "28/05/2024", "14:00 – 16:00", "IELTS-01","IELTS Prep",  "P.203", "10/15", "Đã điểm danh");
        dgv.Rows.Add("Thứ 5", "30/05/2024", "08:00 – 10:00", "E-A1-01", "English A1",  "P.101", "—",     "Chưa điểm danh");
        dgv.Rows.Add("Thứ 6", "31/05/2024", "14:00 – 16:00", "TOE-03",  "TOEIC 600+",  "P.105", "—",     "Chưa điểm danh");
        dgv.Rows.Add("Thứ 7", "01/06/2024", "09:00 – 11:00", "IELTS-01","IELTS Prep",  "P.203", "—",     "Chưa điểm danh");

        // Tô màu theo trạng thái
        dgv.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 7) return;
            var val = dgv.Rows[e.RowIndex].Cells[7].Value?.ToString();
            if (e.CellStyle == null) return;
            e.CellStyle.ForeColor = val == "Đã điểm danh" ? AppTheme.Success : AppTheme.Warning;
            e.CellStyle.Font = AppTheme.FontBodyBold;
        };

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        pnlGrid.Controls.Add(dgv);

        card.Controls.Add(pnlGrid);
        card.Controls.Add(pnlNav);
        card.Controls.Add(pnlHead);
        Controls.Add(card);
    }

    private static void B(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}
