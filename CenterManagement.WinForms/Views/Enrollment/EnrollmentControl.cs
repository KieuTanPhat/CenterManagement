using CenterManagement.Models.DTOs;
using CenterManagement.Models.Enums;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Enrollment;

public class EnrollmentControl : UserControl
{
    private readonly EnrollmentService _service      = new();
    private readonly StudentService    _studentSvc   = new();
    private readonly ClassService      _classSvc     = new();
    private readonly PaymentService    _paymentSvc   = new();
    private DataGridView _grid = new();
    private TextBox  _txtSearch  = new();
    private ComboBox _cmbStatus  = new();
    private Label    _lblStatus  = new();
    private List<EnrollmentDto> _enrollments = new();

    public EnrollmentControl()
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
            Text = "📝  Quản lý Đăng ký học",
            Font = AppTheme.FontSubtitle, ForeColor = AppTheme.TextPrimary,
            Location = new Point(20, 16), AutoSize = true
        });
        var btnNew = AppTheme.MakePrimaryBtn("+ Đăng ký mới", 130, 34);
        btnNew.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnNew.Click += (_, _) => ShowEnrollDialog();
        head.Controls.Add(btnNew);
        head.Resize += (_, _) => btnNew.Location = new Point(head.Width - 150, 13);

        // ── Toolbar ──────────────────────────────────────────────────────────
        var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(250, 250, 252) };
        bar.Paint += BorderBottom;
        _txtSearch = new TextBox
        {
            PlaceholderText = "Tìm học viên, mã HS, lớp học...",
            Location = new Point(16, 12), Size = new Size(260, 28), Font = AppTheme.FontBody
        };
        _cmbStatus = new ComboBox
        {
            Location = new Point(288, 12), Size = new Size(140, 28),
            DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody
        };
        _cmbStatus.Items.AddRange(new object[] { "Tất cả", "Chờ xác nhận", "Đang học", "Hoàn thành", "Đã nghỉ", "Đã chuyển" });
        _cmbStatus.SelectedIndex = 0;
        var btnSearch = AppTheme.MakePrimaryBtn("Tìm", 80, 28);
        btnSearch.Location = new Point(440, 12);
        btnSearch.Click += (_, _) => Filter();
        _txtSearch.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) Filter(); };
        _cmbStatus.SelectedIndexChanged += (_, _) => Filter();
        bar.Controls.AddRange(new Control[] { _txtSearch, _cmbStatus, btnSearch });

        // ── Grid ──────────────────────────────────────────────────────────────
        _grid = AppTheme.MakeGrid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID",          FillWeight = 6  },
            new DataGridViewTextBoxColumn { HeaderText = "Học viên",    FillWeight = 20 },
            new DataGridViewTextBoxColumn { HeaderText = "Mã HS",       FillWeight = 9  },
            new DataGridViewTextBoxColumn { HeaderText = "Lớp học",     FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Khóa học",    FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Ngày ĐK",     FillWeight = 11 },
            new DataGridViewTextBoxColumn { HeaderText = "Học phí",     FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Đã thu",      FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Còn lại",     FillWeight = 11 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái",  FillWeight = 11 }
        );
        _grid.DoubleClick += (_, _) => ViewDetail();

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("📋  Xem chi tiết đăng ký",       null, (_, _) => ViewDetail());
        ctx.Items.Add("💰  Thu học phí cho đăng ký này", null, (_, _) => CollectPayment());
        ctx.Items.Add("-");
        ctx.Items.Add("❌  Hủy đăng ký (yêu cầu mật khẩu)", null, async (_, _) => await CancelSelectedAsync());
        _grid.ContextMenuStrip = ctx;

        _grid.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F5)  { _ = LoadAsync(); e.Handled = true; }
            if (e.Control && e.KeyCode == Keys.N) { ShowEnrollDialog(); e.Handled = true; }
        };

        // ── Footer ──────────────────────────────────────────────────────────
        var foot = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(248, 249, 252) };
        _lblStatus = new Label
        {
            Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary,
            Text = "F5: Làm mới  |  Ctrl+N: Đăng ký mới  |  Double-click: Chi tiết  |  Chuột phải: Menu"
        };
        foot.Controls.Add(_lblStatus);

        card.Controls.Add(_grid);
        card.Controls.Add(foot);
        card.Controls.Add(bar);
        card.Controls.Add(head);
        Controls.Add(card);
    }

    private async Task LoadAsync()
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            _enrollments = await _service.GetAllAsync();
            Filter();
        }
        catch (Exception ex) { _lblStatus.Text = $"Lỗi: {ex.Message}"; }
        finally { Cursor = Cursors.Default; }
    }

    private void Filter()
    {
        var q = _txtSearch.Text.Trim().ToLowerInvariant();
        var list = _enrollments.Where(e =>
            string.IsNullOrEmpty(q) ||
            e.StudentName.ToLowerInvariant().Contains(q) ||
            e.StudentCode.ToLowerInvariant().Contains(q) ||
            e.ClassName.ToLowerInvariant().Contains(q) ||
            e.CourseName.ToLowerInvariant().Contains(q));

        list = _cmbStatus.SelectedIndex switch
        {
            1 => list.Where(e => e.Status == EnrollmentStatus.Pending),
            2 => list.Where(e => e.Status == EnrollmentStatus.Active),
            3 => list.Where(e => e.Status == EnrollmentStatus.Completed),
            4 => list.Where(e => e.Status == EnrollmentStatus.Dropped),
            5 => list.Where(e => e.Status == EnrollmentStatus.Transferred),
            _ => list
        };

        _grid.Rows.Clear();
        foreach (var e in list)
        {
            var remaining = e.TuitionFee - e.TotalPaid;
            _grid.Rows.Add(
                e.Id, e.StudentName, e.StudentCode, e.ClassName, e.CourseName,
                e.EnrollmentDate.ToString("dd/MM/yyyy"),
                $"{e.TuitionFee:N0}đ", $"{e.TotalPaid:N0}đ",
                remaining > 0 ? $"{remaining:N0}đ" : "✅ Đủ",
                StatusText(e.Status)
            );
            _grid.Rows[^1].Tag = e;
            // Tô đỏ nhẹ nếu còn nợ
            if (remaining > 0 && e.Status == EnrollmentStatus.Active)
                _grid.Rows[^1].DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 240);
        }
        var shown = _grid.RowCount;
        _lblStatus.Text = $"Hiển thị {shown} đăng ký.  F5: Làm mới  |  Ctrl+N: Đăng ký mới";
    }

    private EnrollmentDto? Selected() => _grid.CurrentRow?.Tag as EnrollmentDto;

    private void ShowEnrollDialog()
    {
        using var dlg = new NewEnrollmentDialog(_studentSvc, _classSvc, _service);
        if (dlg.ShowDialog(this) == DialogResult.OK) _ = LoadAsync();
    }

    private void ViewDetail()
    {
        if (Selected() is not EnrollmentDto e) return;
        var remaining = e.TuitionFee - e.TotalPaid;
        var info = $"ID đăng ký: {e.Id}\n" +
                   $"Học viên: {e.StudentName}  (Mã: {e.StudentCode})\n" +
                   $"Lớp học: {e.ClassName}\n" +
                   $"Khóa học: {e.CourseName}\n" +
                   $"Ngày đăng ký: {e.EnrollmentDate:dd/MM/yyyy}\n" +
                   $"Học phí: {e.TuitionFee:N0} đồng\n" +
                   $"Đã thu: {e.TotalPaid:N0} đồng\n" +
                   $"Còn lại: {remaining:N0} đồng\n" +
                   $"Trạng thái: {StatusText(e.Status)}";
        MessageBox.Show(info, "Chi tiết đăng ký", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void CollectPayment()
    {
        if (Selected() is not EnrollmentDto e) return;
        using var dlg = new PaymentForEnrollmentDialog(e, _paymentSvc);
        if (dlg.ShowDialog(this) == DialogResult.OK) _ = LoadAsync();
    }

    private async Task CancelSelectedAsync()
    {
        if (Selected() is not EnrollmentDto e) { _lblStatus.Text = "Hãy chọn một đăng ký trước."; return; }
        if (e.Status is EnrollmentStatus.Dropped or EnrollmentStatus.Transferred)
        { _lblStatus.Text = "Đăng ký này đã bị hủy hoặc đã chuyển."; return; }

        if (!PasswordConfirmDialog.Confirm(this,
            $"Nhập mật khẩu để hủy đăng ký của \"{e.StudentName}\" khỏi lớp \"{e.ClassName}\".\n" +
            $"Chính sách hoàn tiền sẽ được áp dụng tự động.")) return;

        var (ok, msg) = await _service.CancelAsync(e.Id, new CancelEnrollmentDto { Reason = "Hủy bởi nhân viên" });
        _lblStatus.Text = ok ? $"✅ Đã hủy đăng ký của {e.StudentName}. Kiểm tra chính sách hoàn tiền." : $"❌ {msg}";
        await LoadAsync();
    }

    private static string StatusText(EnrollmentStatus s) => s switch
    {
        EnrollmentStatus.Pending     => "Chờ xác nhận",
        EnrollmentStatus.Active      => "Đang học",
        EnrollmentStatus.Completed   => "Hoàn thành",
        EnrollmentStatus.Dropped     => "Đã nghỉ",
        EnrollmentStatus.Suspended   => "Tạm dừng",
        EnrollmentStatus.Transferred => "Đã chuyển lớp",
        _                            => s.ToString()
    };

    private static void BorderBottom(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}

// ── Dialog đăng ký học viên mới hoặc học viên hiện có vào lớp ────────────
public sealed class NewEnrollmentDialog : Form
{
    private readonly StudentService   _studentSvc;
    private readonly ClassService     _classSvc;
    private readonly EnrollmentService _enrollSvc;

    // Controls chế độ học viên
    private RadioButton _rdoExisting = new(), _rdoNew = new();
    private Panel _pnlExisting = new(), _pnlNew = new();
    private ComboBox _cmbStudent = new(), _cmbClass = new();

    // Học viên mới
    private TextBox _txtName = new(), _txtPhone = new(), _txtEmail = new(), _txtParent = new(), _txtParentPhone = new();
    private DateTimePicker _dtpDob = new();

    // Thông tin lớp
    private Label _lblTuition = new(), _lblSeats = new(), _lblError = new();

    private List<StudentDto> _studentRows = new();
    private List<ClassDto>   _classRows   = new();

    public NewEnrollmentDialog(StudentService studentSvc, ClassService classSvc, EnrollmentService enrollSvc)
    {
        _studentSvc = studentSvc; _classSvc = classSvc; _enrollSvc = enrollSvc;
        Text = "Đăng ký học viên vào lớp";
        Size = new Size(560, 560);
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
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20, 16, 20, 0) };

        // Chế độ học viên
        var lblMode = new Label { Text = "Học viên:", Font = AppTheme.FontBodyBold, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(0, 0) };
        _rdoExisting = new RadioButton { Text = "Đã có trong hệ thống", Font = AppTheme.FontBody, Location = new Point(0, 24), Checked = true, AutoSize = true };
        _rdoNew      = new RadioButton { Text = "Tạo học viên mới",     Font = AppTheme.FontBody, Location = new Point(200, 24), AutoSize = true };
        _rdoExisting.CheckedChanged += (_, _) => ToggleMode();
        _rdoNew.CheckedChanged      += (_, _) => ToggleMode();

        // Panel học viên có sẵn
        _pnlExisting = new Panel { Location = new Point(0, 56), Size = new Size(500, 40) };
        _cmbStudent = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        _pnlExisting.Controls.Add(_cmbStudent);

        // Panel học viên mới
        _pnlNew = new Panel { Location = new Point(0, 56), Size = new Size(500, 230), Visible = false };
        BuildNewStudentFields(_pnlNew);

        // Panel chọn lớp
        var lblClass = new Label { Text = "Lớp học *", Font = AppTheme.FontBodyBold, ForeColor = AppTheme.TextPrimary, AutoSize = true };
        _cmbClass = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody, Size = new Size(500, 28) };
        _cmbClass.SelectedIndexChanged += (_, _) => UpdateClassInfo();
        _lblTuition = new Label { Font = AppTheme.FontBody, ForeColor = AppTheme.Info, AutoSize = true };
        _lblSeats   = new Label { Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary, AutoSize = true };
        _lblError   = new Label { Font = AppTheme.FontSmall, ForeColor = AppTheme.Danger, AutoSize = true };

        // Layout sử dụng TableLayoutPanel cho phần chọn lớp
        var tblClass = new TableLayoutPanel
        {
            Location = new Point(0, 310), Size = new Size(500, 130),
            ColumnCount = 1, RowCount = 4, AutoSize = true
        };
        tblClass.Controls.Add(lblClass,    0, 0);
        tblClass.Controls.Add(_cmbClass,   0, 1);
        tblClass.Controls.Add(_lblTuition, 0, 2);
        tblClass.Controls.Add(_lblSeats,   0, 3);

        scroll.Controls.AddRange(new Control[]
        {
            lblMode, _rdoExisting, _rdoNew,
            _pnlExisting, _pnlNew,
            tblClass, _lblError
        });
        _lblError.Location = new Point(0, 450);

        var foot = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 16, 0) };
        var btnSave   = AppTheme.MakePrimaryBtn("Đăng ký", 100, 34);
        var btnCancel = AppTheme.MakeOutlineBtn("Hủy", 90, 34);
        btnSave.Click   += async (_, _) => await SaveAsync();
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        foot.Controls.AddRange(new Control[] { btnSave, btnCancel });

        Controls.Add(scroll);
        Controls.Add(foot);
    }

    private void BuildNewStudentFields(Panel p)
    {
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6 };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void Row(string lbl, Control ctrl, int r)
        {
            tbl.Controls.Add(new Label { Text = lbl, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, r);
            ctrl.Dock = DockStyle.Fill; ctrl.Font = AppTheme.FontBody;
            tbl.Controls.Add(ctrl, 1, r);
        }
        _dtpDob = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddYears(-18) };
        Row("Họ và tên *", _txtName = new TextBox(), 0);
        Row("Ngày sinh",   _dtpDob, 1);
        Row("Điện thoại",  _txtPhone = new TextBox(), 2);
        Row("Email",       _txtEmail = new TextBox(), 3);
        Row("Tên phụ huynh", _txtParent = new TextBox(), 4);
        Row("SĐT phụ huynh", _txtParentPhone = new TextBox(), 5);
        p.Controls.Add(tbl);
    }

    private void ToggleMode()
    {
        bool isNew = _rdoNew.Checked;
        _pnlExisting.Visible = !isNew;
        _pnlNew.Visible      = isNew;
    }

    private async Task LoadAsync()
    {
        _studentRows = await _studentSvc.GetAllAsync();
        _classRows   = await _classSvc.GetAllAsync(ClassStatus.Upcoming);
        // Thêm cả lớp Active
        var active = await _classSvc.GetAllAsync(ClassStatus.Active);
        _classRows.AddRange(active);

        _cmbStudent.DataSource    = _studentRows;
        _cmbStudent.DisplayMember = nameof(StudentDto.FullName);
        _cmbStudent.ValueMember   = nameof(StudentDto.Id);

        _cmbClass.DataSource    = _classRows;
        _cmbClass.DisplayMember = nameof(ClassDto.ClassName);
        _cmbClass.ValueMember   = nameof(ClassDto.Id);
        UpdateClassInfo();
    }

    private void UpdateClassInfo()
    {
        if (_cmbClass.SelectedItem is ClassDto c)
        {
            _lblTuition.Text = $"Học phí: (xem chi tiết sau khi API trả về)  |  Đặt cọc 20%";
            _lblSeats.Text   = $"Số chỗ: {c.EnrolledCount}/{c.MaxStudents}  ({c.MaxStudents - c.EnrolledCount} chỗ còn trống)";
            _lblSeats.ForeColor = c.EnrolledCount >= c.MaxStudents ? AppTheme.Danger : AppTheme.TextSecondary;
        }
    }

    private async Task SaveAsync()
    {
        _lblError.Text = "";

        // Kiểm tra chọn lớp
        if (_cmbClass.SelectedItem is not ClassDto cls)
        { _lblError.Text = "Vui lòng chọn lớp học."; return; }

        if (cls.EnrolledCount >= cls.MaxStudents)
        { _lblError.Text = "Lớp đã đầy, không thể đăng ký thêm."; return; }

        int studentId;
        if (_rdoNew.Checked)
        {
            // Tạo học viên mới
            if (string.IsNullOrWhiteSpace(_txtName.Text)) { _lblError.Text = "Vui lòng nhập họ tên học viên."; return; }
            static string? NE(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
            var created = await _studentSvc.CreateAndReturnAsync(new CreateStudentDto
            {
                FullName    = _txtName.Text.Trim(),
                Phone       = NE(_txtPhone.Text),
                Email       = NE(_txtEmail.Text),
                DateOfBirth = DateOnly.FromDateTime(_dtpDob.Value),
                ParentName  = NE(_txtParent.Text),
                ParentPhone = NE(_txtParentPhone.Text)
            });
            if (created == null) { _lblError.Text = "Tạo học viên thất bại. Kiểm tra dữ liệu (số điện thoại/email đã tồn tại?)."; return; }
            studentId = created.Id;
        }
        else
        {
            if (_cmbStudent.SelectedItem is not StudentDto stu) { _lblError.Text = "Vui lòng chọn học viên."; return; }
            studentId = stu.Id;
        }

        var (ok, msg, _) = await _enrollSvc.EnrollAsync(new CreateEnrollmentDto { StudentId = studentId, ClassId = cls.Id });
        if (ok) DialogResult = DialogResult.OK;
        else _lblError.Text = $"Đăng ký thất bại: {msg}";
    }
}

// ── Dialog thu học phí cho một đăng ký cụ thể ────────────────────────────
internal sealed class PaymentForEnrollmentDialog : Form
{
    private readonly EnrollmentDto _enrollment;
    private readonly PaymentService _paymentSvc;
    private NumericUpDown _numAmount = new();
    private ComboBox _cmbMethod = new(), _cmbType = new();
    private TextBox  _txtNote   = new();
    private Label    _lblError  = new();

    public PaymentForEnrollmentDialog(EnrollmentDto enrollment, PaymentService paymentSvc)
    {
        _enrollment = enrollment; _paymentSvc = paymentSvc;
        Text = $"Thu học phí — {enrollment.StudentName} / {enrollment.ClassName}";
        Size = new Size(480, 320);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.White;
        Build();
    }

    private void Build()
    {
        var remaining = _enrollment.TuitionFee - _enrollment.TotalPaid;
        var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 7 };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void Row(string lbl, Control ctrl, int r)
        {
            tbl.Controls.Add(new Label { Text = lbl, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, r);
            ctrl.Dock = DockStyle.Fill; ctrl.Font = AppTheme.FontBody;
            tbl.Controls.Add(ctrl, 1, r);
        }

        var lblInfo = new Label
        {
            Text = $"Học phí: {_enrollment.TuitionFee:N0}đ  |  Đã thu: {_enrollment.TotalPaid:N0}đ  |  Còn lại: {remaining:N0}đ",
            Font = AppTheme.FontBodyBold, ForeColor = remaining > 0 ? AppTheme.Warning : AppTheme.Success,
            Dock = DockStyle.Fill
        };
        tbl.Controls.Add(lblInfo, 0, 0);
        tbl.SetColumnSpan(lblInfo, 2);

        _numAmount = new NumericUpDown { Maximum = 100_000_000, ThousandsSeparator = true, DecimalPlaces = 0, Value = Math.Max(0, remaining) };
        _cmbMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbMethod.DataSource = Enum.GetValues(typeof(PaymentMethod));
        _cmbType   = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbType.Items.AddRange(new object[] { "Deposit", "Remaining", "ExamFee" });
        _cmbType.SelectedIndex = _enrollment.TotalPaid == 0 ? 0 : 1;
        _txtNote = new TextBox { PlaceholderText = "Ghi chú (tùy chọn)" };

        Row("Số tiền *", _numAmount, 1);
        Row("Phương thức", _cmbMethod, 2);
        Row("Loại thu", _cmbType, 3);
        Row("Ghi chú", _txtNote, 4);

        _lblError = new Label { ForeColor = AppTheme.Danger, Font = AppTheme.FontSmall, AutoSize = true };
        tbl.Controls.Add(_lblError, 0, 5);
        tbl.SetColumnSpan(_lblError, 2);

        var foot = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 16, 0) };
        var btnSave   = AppTheme.MakePrimaryBtn("Xác nhận thu", 120, 34);
        var btnCancel = AppTheme.MakeOutlineBtn("Hủy", 90, 34);
        btnSave.Click   += async (_, _) => await SaveAsync();
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        foot.Controls.AddRange(new Control[] { btnSave, btnCancel });

        Controls.Add(tbl);
        Controls.Add(foot);
    }

    private async Task SaveAsync()
    {
        if (_numAmount.Value <= 0) { _lblError.Text = "Số tiền phải lớn hơn 0."; return; }
        var (ok, msg) = await _paymentSvc.CreatePaymentAsync(new CreatePaymentDto
        {
            EnrollmentId  = _enrollment.Id,
            Amount        = _numAmount.Value,
            PaymentMethod = (PaymentMethod)_cmbMethod.SelectedItem!,
            PaymentType   = _cmbType.SelectedItem?.ToString() ?? "Deposit",
            Note          = string.IsNullOrWhiteSpace(_txtNote.Text) ? null : _txtNote.Text.Trim()
        });
        if (ok) DialogResult = DialogResult.OK;
        else _lblError.Text = $"Thu thất bại: {msg}";
    }
}
