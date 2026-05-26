using CenterManagement.WinForms.Core;

namespace CenterManagement.WinForms.Views.Grades;

/// <summary>
/// Role 4 (Teacher): nhập điểm học sinh theo lớp.
/// Role 1,2: xem bảng điểm tổng hợp.
/// </summary>
public class GradeControl : UserControl
{
    private readonly int _roleId;

    public GradeControl(int roleId = 1)
    {
        _roleId = roleId;
        BackColor = AppTheme.ContentBg;
        Dock      = DockStyle.Fill;
        BuildUI();
    }

    private void BuildUI()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        // ── Header ─────────────────────────────────────────────────────────
        var pnlHead = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        pnlHead.Paint += Border;
        var title = _roleId == 4 ? "Nhập điểm học sinh" : "Bảng điểm học sinh";
        pnlHead.Controls.Add(new Label
        {
            Text = title, Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true
        });

        if (_roleId != 4)
        {
            var btnExport = AppTheme.MakeOutlineBtn("Xuất Excel", 110, 36);
            btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnlHead.Controls.Add(btnExport);
            pnlHead.Resize += (s, e) => btnExport.Location = new Point(pnlHead.Width - 130, 12);
        }

        // ── Bộ chọn lớp / bài kiểm tra ────────────────────────────────────
        var pnlSel = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.FromArgb(250, 250, 252) };
        pnlSel.Paint += Border;

        var cmbClass = new ComboBox
        {
            Location = new Point(16, 16), Size = new Size(220, 28),
            DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody
        };
        cmbClass.Items.AddRange(new object[] { "E-A1-01 – English A1", "IELTS-01 – IELTS Prep", "TOE-03 – TOEIC 600+" });
        cmbClass.SelectedIndex = 0;

        var cmbExam = new ComboBox
        {
            Location = new Point(250, 16), Size = new Size(200, 28),
            DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody
        };
        cmbExam.Items.AddRange(new object[] { "Kiểm tra giữa kỳ", "Kiểm tra cuối kỳ", "Bài tập 1", "Bài tập 2" });
        cmbExam.SelectedIndex = 0;

        var btnLoad = AppTheme.MakePrimaryBtn("Tải danh sách", 130, 32);
        btnLoad.Location = new Point(464, 16);

        pnlSel.Controls.AddRange(new Control[] { cmbClass, cmbExam, btnLoad });

        // ── Bảng nhập / xem điểm ──────────────────────────────────────────
        var dgv = AppTheme.MakeGrid();
        dgv.ReadOnly = _roleId != 4;
        dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "STT",          FillWeight = 6,  ReadOnly = true },
            new DataGridViewTextBoxColumn { HeaderText = "Mã HS",         FillWeight = 10, ReadOnly = true },
            new DataGridViewTextBoxColumn { HeaderText = "Họ và tên",      FillWeight = 26, ReadOnly = true },
            new DataGridViewTextBoxColumn { HeaderText = "Nghe",          FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Nói",           FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Đọc",           FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Viết",          FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Tổng",          FillWeight = 10, ReadOnly = true },
            new DataGridViewTextBoxColumn { HeaderText = "Xếp loại",      FillWeight = 8,  ReadOnly = true }
        );

        dgv.Rows.Add("1", "HS001", "Nguyễn Văn An",   "8.5", "7.0", "8.0", "8.5", "8.0",  "Giỏi");
        dgv.Rows.Add("2", "HS002", "Trần Thị Bình",   "7.0", "8.5", "7.5", "7.0", "7.5",  "Khá");
        dgv.Rows.Add("3", "HS003", "Lê Quốc Cường",   "5.5", "6.0", "5.0", "6.5", "5.75", "Trung bình");
        dgv.Rows.Add("4", "HS004", "Phạm Thị Dung",   "9.0", "8.5", "9.5", "9.0", "9.0",  "Xuất sắc");
        dgv.Rows.Add("5", "HS005", "Hoàng Minh Đức",  "6.5", "7.0", "6.0", "7.5", "6.75", "Khá");

        // ── Footer ─────────────────────────────────────────────────────────
        var pnlFoot = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.White };
        pnlFoot.Paint += (s, e) =>
        {
            if (s is not Control c) return;
            using var p = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(p, 0, 0, c.Width, 0);
        };

        var lblStats = new Label
        {
            Text = "Xuất sắc: 1  |  Giỏi: 1  |  Khá: 2  |  Trung bình: 1  |  Điểm TB lớp: 7.4",
            Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary,
            Location = new Point(16, 0), Size = new Size(500, 52),
            TextAlign = ContentAlignment.MiddleLeft
        };
        pnlFoot.Controls.Add(lblStats);

        if (_roleId == 4)
        {
            var btnSave = AppTheme.MakePrimaryBtn("Lưu điểm", 110, 36);
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnlFoot.Controls.Add(btnSave);
            pnlFoot.Resize += (s, e) => btnSave.Location = new Point(pnlFoot.Width - 126, 8);
        }

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        pnlGrid.Controls.Add(dgv);

        card.Controls.Add(pnlGrid);
        card.Controls.Add(pnlFoot);
        card.Controls.Add(pnlSel);
        card.Controls.Add(pnlHead);
        Controls.Add(card);
    }

    private static void Border(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}
