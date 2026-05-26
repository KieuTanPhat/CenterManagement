using CenterManagement.WinForms.Core;

namespace CenterManagement.WinForms.Views.Reports;

public class ReportControl : UserControl
{
    public ReportControl()
    {
        BackColor = AppTheme.ContentBg;
        Dock      = DockStyle.Fill;
        BuildUI();
    }

    private void BuildUI()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        // ── Header ─────────────────────────────────────────────────────────
        var pnlHead = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        pnlHead.Paint += B;
        pnlHead.Controls.Add(new Label
        {
            Text = "Thống kê & Báo cáo", Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true
        });
        var btnExport = AppTheme.MakeOutlineBtn("Xuất báo cáo", 130, 36);
        btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        pnlHead.Controls.Add(btnExport);
        pnlHead.Resize += (s, e) => btnExport.Location = new Point(pnlHead.Width - 150, 12);

        // ── Bộ lọc thời gian ───────────────────────────────────────────────
        var pnlFilter = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(250, 250, 252) };
        pnlFilter.Paint += B;

        var cmbPeriod = new ComboBox
        {
            Location = new Point(16, 16), Size = new Size(160, 28),
            DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody
        };
        cmbPeriod.Items.AddRange(new object[] { "Tháng này", "Tháng trước", "Quý này", "Năm nay", "Tuỳ chọn" });
        cmbPeriod.SelectedIndex = 0;

        var dtpFrom = new DateTimePicker { Location = new Point(190, 16), Size = new Size(140, 28), Format = DateTimePickerFormat.Short };
        var lbTo    = new Label { Text = "–", Location = new Point(338, 20), AutoSize = true, Font = AppTheme.FontBody };
        var dtpTo   = new DateTimePicker { Location = new Point(354, 16), Size = new Size(140, 28), Format = DateTimePickerFormat.Short };
        var btnApply= AppTheme.MakePrimaryBtn("Áp dụng", 100, 28);
        btnApply.Location = new Point(508, 16);

        pnlFilter.Controls.AddRange(new Control[] { cmbPeriod, dtpFrom, lbTo, dtpTo, btnApply });

        // ── Tabs báo cáo ───────────────────────────────────────────────────
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = AppTheme.FontBody
        };

        tabs.TabPages.Add(BuildDoanhThuTab());
        tabs.TabPages.Add(BuildHocSinhTab());
        tabs.TabPages.Add(BuildDiemDanhTab());
        tabs.TabPages.Add(BuildLopHocTab());

        card.Controls.Add(tabs);
        card.Controls.Add(pnlFilter);
        card.Controls.Add(pnlHead);
        Controls.Add(card);
    }

    private static TabPage BuildDoanhThuTab()
    {
        var tab = new TabPage("  Doanh thu  ") { BackColor = Color.White };

        // Summary cards
        var pnlCards = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Color.White, Padding = new Padding(8) };

        int cx = 8;
        foreach (var (v, t, c) in new[] {
            ("42.500.000 đ", "Doanh thu tháng 5",   AppTheme.Primary),
            ("38.200.000 đ", "Doanh thu tháng 4",   AppTheme.Info),
            ("23",           "Thanh toán hoàn tất",  AppTheme.Success),
            ("5",            "Chưa thanh toán",      AppTheme.Danger) })
        {
            var card = MiniCard(v, t, c);
            card.Location = new Point(cx, 8);
            pnlCards.Controls.Add(card);
            cx += 224;
        }

        var dgv = AppTheme.MakeGrid();
        dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Tháng",         FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Số học sinh",   FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Doanh thu",     FillWeight = 20 },
            new DataGridViewTextBoxColumn { HeaderText = "Đã thu",        FillWeight = 20 },
            new DataGridViewTextBoxColumn { HeaderText = "Còn nợ",        FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Tỉ lệ thu",     FillWeight = 16 }
        );
        dgv.Rows.Add("Tháng 1/2024", "210", "36.000.000 đ", "35.200.000 đ", "800.000 đ",   "97.8%");
        dgv.Rows.Add("Tháng 2/2024", "215", "37.500.000 đ", "36.800.000 đ", "700.000 đ",   "98.1%");
        dgv.Rows.Add("Tháng 3/2024", "220", "39.000.000 đ", "38.100.000 đ", "900.000 đ",   "97.7%");
        dgv.Rows.Add("Tháng 4/2024", "235", "38.200.000 đ", "38.200.000 đ", "0 đ",          "100%");
        dgv.Rows.Add("Tháng 5/2024", "248", "42.500.000 đ", "38.700.000 đ", "3.800.000 đ", "91.1%");

        var pGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        pGrid.Controls.Add(dgv);
        tab.Controls.Add(pGrid);
        tab.Controls.Add(pnlCards);
        return tab;
    }

    private static TabPage BuildHocSinhTab()
    {
        var tab = new TabPage("  Học sinh  ") { BackColor = Color.White };
        var dgv = AppTheme.MakeGrid();
        dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Tháng",           FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Đăng ký mới",     FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Bảo lưu",         FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Nghỉ học",        FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Tổng đang học",   FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Tăng trưởng",     FillWeight = 14 }
        );
        dgv.Rows.Add("Tháng 3/2024", "28", "3", "2", "220", "+12%");
        dgv.Rows.Add("Tháng 4/2024", "22", "2", "5", "235", "+6.8%");
        dgv.Rows.Add("Tháng 5/2024", "18", "4", "1", "248", "+5.5%");
        var pg = new Panel { Dock = DockStyle.Fill }; pg.Controls.Add(dgv);
        tab.Controls.Add(pg);
        return tab;
    }

    private static TabPage BuildDiemDanhTab()
    {
        var tab = new TabPage("  Điểm danh  ") { BackColor = Color.White };
        var dgv = AppTheme.MakeGrid();
        dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Lớp học",        FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Tổng buổi",      FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Tỉ lệ đến",      FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Vắng nhiều nhất",FillWeight = 24 },
            new DataGridViewTextBoxColumn { HeaderText = "Số buổi vắng",   FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Ghi chú",        FillWeight = 20 }
        );
        dgv.Rows.Add("E-A1-01",  "24", "92%",  "Lê Quốc Cường",   "5", "Liên hệ phụ huynh");
        dgv.Rows.Add("E-B2-02",  "20", "96%",  "Trần Thị Mai",     "2", "");
        dgv.Rows.Add("IELTS-01", "16", "88%",  "Hoàng Văn Khoa",   "4", "Cần theo dõi");
        var pg = new Panel { Dock = DockStyle.Fill }; pg.Controls.Add(dgv);
        tab.Controls.Add(pg);
        return tab;
    }

    private static TabPage BuildLopHocTab()
    {
        var tab = new TabPage("  Lớp học  ") { BackColor = Color.White };
        var dgv = AppTheme.MakeGrid();
        dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Khóa học",       FillWeight = 22 },
            new DataGridViewTextBoxColumn { HeaderText = "Số lớp mở",      FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Tổng học viên",  FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Đã hoàn thành",  FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Đang học",       FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Sắp khai giảng", FillWeight = 14 }
        );
        dgv.Rows.Add("English A1",  "4", "62", "1 lớp", "3 lớp", "0");
        dgv.Rows.Add("English B2",  "3", "55", "0",     "3 lớp", "0");
        dgv.Rows.Add("IELTS Prep",  "2", "28", "0",     "2 lớp", "0");
        dgv.Rows.Add("TOEIC 600+",  "2", "38", "1 lớp", "1 lớp", "1 lớp");
        var pg = new Panel { Dock = DockStyle.Fill }; pg.Controls.Add(dgv);
        tab.Controls.Add(pg);
        return tab;
    }

    private static Panel MiniCard(string value, string title, Color accent)
    {
        var c = new Panel { Size = new Size(210, 90), BackColor = Color.White };
        c.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(4, 90), BackColor = accent });
        c.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = AppTheme.TextPrimary, Location = new Point(14, 12), AutoSize = true });
        c.Controls.Add(new Label { Text = title,  Font = AppTheme.FontSmall,    ForeColor = AppTheme.TextSecondary,  Location = new Point(14, 58), AutoSize = true });
        return c;
    }

    private static void B(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}
