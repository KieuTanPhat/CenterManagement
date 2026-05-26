using CenterManagement.Models.DTOs;
using CenterManagement.Models.Enums;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Payments;

public class PaymentControl : UserControl
{
    private readonly PaymentService    _paymentSvc    = new();
    private readonly EnrollmentService _enrollmentSvc = new();
    private DataGridView _grid = new();
    private Label _lblStatus = new();
    private Label _lblTotal = new(), _lblDeposit = new(), _lblRemaining = new();
    private ComboBox _cmbMonth = new(), _cmbYear = new(), _cmbStatus = new();
    private List<PaymentDto> _payments = new();

    public PaymentControl()
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

        // ── Header ──────────────────────────────────────────────────────────
        var head = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        head.Paint += BorderBottom;
        head.Controls.Add(new Label
        {
            Text = "💰  Thu học phí",
            Font = AppTheme.FontSubtitle, ForeColor = AppTheme.TextPrimary,
            Location = new Point(20, 16), AutoSize = true
        });
        var btnCollect = AppTheme.MakePrimaryBtn("Thu học phí", 120, 34);
        btnCollect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCollect.Click += (_, _) => ShowCollectDialog();
        head.Controls.Add(btnCollect);
        head.Resize += (_, _) => btnCollect.Location = new Point(head.Width - 140, 13);

        // ── Bộ lọc ──────────────────────────────────────────────────────────
        var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(250, 250, 252) };
        bar.Paint += BorderBottom;

        var lblMonth = new Label { Text = "Tháng:", Location = new Point(16, 16), AutoSize = true, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary };
        _cmbMonth = new ComboBox { Location = new Point(65, 12), Size = new Size(70, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        for (int m = 1; m <= 12; m++) _cmbMonth.Items.Add($"T{m}");
        _cmbMonth.SelectedIndex = DateTime.Now.Month - 1;

        var lblYear = new Label { Text = "Năm:", Location = new Point(147, 16), AutoSize = true, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary };
        _cmbYear = new ComboBox { Location = new Point(182, 12), Size = new Size(80, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        for (int y = DateTime.Now.Year - 2; y <= DateTime.Now.Year + 1; y++) _cmbYear.Items.Add(y.ToString());
        _cmbYear.SelectedItem = DateTime.Now.Year.ToString();

        var lblSt = new Label { Text = "Trạng thái:", Location = new Point(275, 16), AutoSize = true, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary };
        _cmbStatus = new ComboBox { Location = new Point(350, 12), Size = new Size(130, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        _cmbStatus.Items.AddRange(new object[] { "Tất cả", "Chờ xử lý", "Đã thu", "Thất bại", "Đã hoàn" });
        _cmbStatus.SelectedIndex = 0;

        var btnFilter = AppTheme.MakePrimaryBtn("Lọc", 70, 28);
        btnFilter.Location = new Point(492, 12);
        btnFilter.Click += async (_, _) => await LoadAsync();
        _cmbStatus.SelectedIndexChanged += async (_, _) => await LoadAsync();

        bar.Controls.AddRange(new Control[] { lblMonth, _cmbMonth, lblYear, _cmbYear, lblSt, _cmbStatus, btnFilter });

        // ── Thẻ tổng kết ──────────────────────────────────────────────────────
        var pnlCards = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(248, 249, 252) };
        _lblTotal     = new Label { Text = "—", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = AppTheme.Success };
        _lblDeposit   = new Label { Text = "—", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = AppTheme.Info };
        _lblRemaining = new Label { Text = "—", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = AppTheme.Warning };
        var cds = new[]
        {
            MakeCard("Tổng thu trong tháng", _lblTotal,     AppTheme.Success),
            MakeCard("Thu từ đặt cọc",        _lblDeposit,   AppTheme.Info),
            MakeCard("Thu học phí",           _lblRemaining, AppTheme.Warning)
        };
        int cx = 12;
        foreach (var c in cds) { c.Location = new Point(cx, 8); pnlCards.Controls.Add(c); cx += c.Width + 14; }

        // ── Grid ──────────────────────────────────────────────────────────────
        _grid = AppTheme.MakeGrid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID",         FillWeight = 6  },
            new DataGridViewTextBoxColumn { HeaderText = "Học viên",   FillWeight = 20 },
            new DataGridViewTextBoxColumn { HeaderText = "Lớp học",    FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Loại thu",   FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Số tiền",    FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Phương thức",FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Ngày thu",   FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Ghi chú",    FillWeight = 18 }
        );
        _grid.DoubleClick += (_, _) => ViewDetail();

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("📋  Xem chi tiết giao dịch", null, (_, _) => ViewDetail());
        _grid.ContextMenuStrip = ctx;

        _grid.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F5)  { _ = LoadAsync(); e.Handled = true; }
            if (e.Control && e.KeyCode == Keys.N) { ShowCollectDialog(); e.Handled = true; }
        };

        // ── Footer ──────────────────────────────────────────────────────────
        var foot = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(248, 249, 252) };
        _lblStatus = new Label
        {
            Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary,
            Text = "F5: Làm mới  |  Ctrl+N: Thu học phí  |  Double-click: Chi tiết  |  Chuột phải: Menu"
        };
        foot.Controls.Add(_lblStatus);

        card.Controls.Add(_grid);
        card.Controls.Add(foot);
        card.Controls.Add(pnlCards);
        card.Controls.Add(bar);
        card.Controls.Add(head);
        Controls.Add(card);
    }

    private async Task LoadAsync()
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            var month = _cmbMonth.SelectedIndex + 1;
            var year  = int.Parse(_cmbYear.SelectedItem?.ToString() ?? DateTime.Now.Year.ToString());

            // Lấy tổng kết
            var summary = await _paymentSvc.GetSummaryAsync(month, year);
            if (summary != null)
            {
                _lblTotal.Text     = $"{summary.TotalRevenue:N0} đ";
                _lblDeposit.Text   = $"{summary.TotalDeposit:N0} đ";
                _lblRemaining.Text = $"{summary.TotalRemaining:N0} đ";
            }

            // Lấy danh sách
            PaymentStatus? statusFilter = _cmbStatus.SelectedIndex switch
            {
                1 => PaymentStatus.Pending,
                2 => PaymentStatus.Completed,
                3 => PaymentStatus.Failed,
                4 => PaymentStatus.Refunded,
                _ => null
            };
            _payments = await _paymentSvc.GetAllAsync(status: statusFilter);

            // Lọc theo tháng/năm
            _payments = _payments.Where(p => p.PaymentDate.Month == month && p.PaymentDate.Year == year).ToList();

            RefreshGrid();
        }
        catch (Exception ex) { _lblStatus.Text = $"Lỗi: {ex.Message}"; }
        finally { Cursor = Cursors.Default; }
    }

    private void RefreshGrid()
    {
        _grid.Rows.Clear();
        foreach (var p in _payments)
        {
            _grid.Rows.Add(
                p.Id, p.StudentName, p.ClassName,
                TypeLabel(p.PaymentType),
                $"{p.Amount:N0}đ",
                MethodText(p.PaymentMethod),
                p.PaymentDate.ToString("dd/MM/yyyy"),
                StatusText(p.Status),
                p.Note ?? ""
            );
            _grid.Rows[^1].Tag = p;
            _grid.Rows[^1].DefaultCellStyle.ForeColor = p.Status switch
            {
                PaymentStatus.Completed => AppTheme.TextPrimary,
                PaymentStatus.Refunded  => AppTheme.Warning,
                PaymentStatus.Failed    => AppTheme.Danger,
                _                       => AppTheme.TextSecondary
            };
        }
        _lblStatus.Text = $"Hiển thị {_payments.Count} giao dịch.  F5: Làm mới  |  Ctrl+N: Thu học phí";
    }

    private void ShowCollectDialog()
    {
        using var form = new PaymentCreateDialog(_paymentSvc, _enrollmentSvc);
        if (form.ShowDialog(this) == DialogResult.OK) _ = LoadAsync();
    }

    private void ViewDetail()
    {
        if (_grid.CurrentRow?.Tag is not PaymentDto p) return;
        var info = $"ID: {p.Id}\nHọc viên: {p.StudentName}\nLớp: {p.ClassName}\n" +
                   $"Loại: {TypeLabel(p.PaymentType)}\nSố tiền: {p.Amount:N0} đồng\n" +
                   $"Phương thức: {MethodText(p.PaymentMethod)}\n" +
                   $"Ngày thu: {p.PaymentDate:dd/MM/yyyy}\n" +
                   $"Trạng thái: {StatusText(p.Status)}\n" +
                   $"Ghi chú: {p.Note ?? "Không có"}";
        MessageBox.Show(info, "Chi tiết giao dịch", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static Panel MakeCard(string title, Label valLabel, Color accent)
    {
        var card = new Panel { Size = new Size(190, 74), BackColor = Color.White };
        card.Paint += (s, e) => { using var pen = new Pen(AppTheme.Border); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };
        var bar = new Panel { Location = new Point(0, 0), Size = new Size(4, 74), BackColor = accent };
        valLabel.Location = new Point(12, 10); valLabel.AutoSize = true;
        var lbl = new Label { Text = title, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary, Location = new Point(12, 48), AutoSize = true };
        card.Controls.AddRange(new Control[] { bar, valLabel, lbl });
        return card;
    }

    private static string TypeLabel(string? t) => t switch
    {
        "Deposit"   => "Đặt cọc",
        "Remaining" => "Học phí",
        "ExamFee"   => "Phí thi",
        "Refund"    => "Hoàn tiền",
        _           => t ?? "—"
    };

    private static string MethodText(PaymentMethod m) => m switch
    {
        PaymentMethod.Cash         => "Tiền mặt",
        PaymentMethod.BankTransfer => "Chuyển khoản",
        PaymentMethod.Card         => "Thẻ",
        _                          => m.ToString()
    };

    private static string StatusText(PaymentStatus s) => s switch
    {
        PaymentStatus.Pending   => "Chờ xử lý",
        PaymentStatus.Completed => "Đã thu",
        PaymentStatus.Failed    => "Thất bại",
        PaymentStatus.Refunded  => "Đã hoàn",
        _                       => s.ToString()
    };

    private static void BorderBottom(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}

// ── Dialog thu học phí (chọn từ danh sách đăng ký) ───────────────────────
internal sealed class PaymentCreateDialog : Form
{
    private readonly PaymentService    _payments;
    private readonly EnrollmentService _enrollments;
    private ComboBox      _cmbEnroll = new();
    private NumericUpDown _numAmount = new();
    private ComboBox      _cmbMethod = new(), _cmbType = new();
    private TextBox       _txtNote   = new();
    private Label         _lblInfo   = new(), _lblError = new();
    private List<EnrollmentDto> _enrollmentRows = new();

    public PaymentCreateDialog(PaymentService payments, EnrollmentService enrollments)
    {
        _payments = payments; _enrollments = enrollments;
        Text = "Thu học phí";
        Size = new Size(520, 360);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.White;
        Build();
        if (!DesignModeHelper.IsInDesignMode)
            _ = LoadAsync();
    }

    private void Build()
    {
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 8 };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void Row(string lbl, Control ctrl, int r)
        {
            tbl.Controls.Add(new Label { Text = lbl, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, r);
            ctrl.Dock = DockStyle.Fill; ctrl.Font = AppTheme.FontBody;
            tbl.Controls.Add(ctrl, 1, r);
        }

        _cmbEnroll = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbEnroll.SelectedIndexChanged += UpdateInfo;
        _numAmount = new NumericUpDown { Maximum = 100_000_000, ThousandsSeparator = true, DecimalPlaces = 0 };
        _cmbMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbMethod.DataSource = Enum.GetValues(typeof(PaymentMethod));
        _cmbType   = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbType.Items.AddRange(new object[] { "Deposit", "Remaining", "ExamFee" });
        _cmbType.SelectedIndex = 0;
        _txtNote = new TextBox { PlaceholderText = "Ghi chú (tùy chọn)" };
        _lblInfo = new Label { Font = AppTheme.FontBodyBold, ForeColor = AppTheme.Info, Dock = DockStyle.Fill };

        Row("Đăng ký *",   _cmbEnroll, 0);
        tbl.Controls.Add(_lblInfo, 0, 1); tbl.SetColumnSpan(_lblInfo, 2);
        Row("Số tiền *",   _numAmount, 2);
        Row("Phương thức", _cmbMethod, 3);
        Row("Loại thu",    _cmbType,   4);
        Row("Ghi chú",     _txtNote,   5);

        _lblError = new Label { ForeColor = AppTheme.Danger, Font = AppTheme.FontSmall, AutoSize = true };
        tbl.Controls.Add(_lblError, 0, 6); tbl.SetColumnSpan(_lblError, 2);

        var foot = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 16, 0) };
        var btnSave   = AppTheme.MakePrimaryBtn("Xác nhận thu", 130, 34);
        var btnCancel = AppTheme.MakeOutlineBtn("Hủy", 90, 34);
        btnSave.Click   += async (_, _) => await SaveAsync();
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        foot.Controls.AddRange(new Control[] { btnSave, btnCancel });

        Controls.Add(tbl);
        Controls.Add(foot);
    }

    private async Task LoadAsync()
    {
        _enrollmentRows = await _enrollments.GetAllAsync();
        // Chỉ hiện các đăng ký chưa đóng đủ tiền
        _enrollmentRows = _enrollmentRows
            .Where(e => e.Status is EnrollmentStatus.Active or EnrollmentStatus.Pending)
            .ToList();
        _cmbEnroll.DataSource    = _enrollmentRows;
        _cmbEnroll.DisplayMember = nameof(EnrollmentDto.StudentName);
        _cmbEnroll.ValueMember   = nameof(EnrollmentDto.Id);
        UpdateInfo(null, EventArgs.Empty);
    }

    private void UpdateInfo(object? s, EventArgs e)
    {
        if (_cmbEnroll.SelectedItem is not EnrollmentDto en) return;
        var remaining = en.TuitionFee - en.TotalPaid;
        _lblInfo.Text = $"Học phí: {en.TuitionFee:N0}đ  |  Đã thu: {en.TotalPaid:N0}đ  |  Còn lại: {remaining:N0}đ";
        _lblInfo.ForeColor = remaining > 0 ? AppTheme.Warning : AppTheme.Success;
        _numAmount.Value = Math.Max(0, Math.Min(_numAmount.Maximum, remaining));
        // Gợi ý loại thu
        _cmbType.SelectedIndex = en.TotalPaid == 0 ? 0 : 1;
    }

    private async Task SaveAsync()
    {
        if (_cmbEnroll.SelectedItem is not EnrollmentDto en || _numAmount.Value <= 0)
        { _lblError.Text = "Vui lòng chọn đăng ký và nhập số tiền > 0."; return; }

        var (ok, msg) = await _payments.CreatePaymentAsync(new CreatePaymentDto
        {
            EnrollmentId  = en.Id,
            Amount        = _numAmount.Value,
            PaymentMethod = (PaymentMethod)_cmbMethod.SelectedItem!,
            PaymentType   = _cmbType.SelectedItem?.ToString() ?? "Deposit",
            Note          = string.IsNullOrWhiteSpace(_txtNote.Text) ? null : _txtNote.Text.Trim()
        });
        if (ok) DialogResult = DialogResult.OK;
        else _lblError.Text = $"Thu thất bại: {msg}";
    }
}
