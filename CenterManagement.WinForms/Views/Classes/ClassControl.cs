using CenterManagement.Models.DTOs;
using CenterManagement.Models.Enums;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Classes;

public class ClassControl : UserControl
{
    private readonly ClassService _service  = new();
    private readonly CourseService _courses = new();
    private readonly int _roleId;
    private DataGridView _grid = new();
    private ComboBox _cmbStatus = new();
    private TextBox _txtSearch = new();
    private Label _lblStatus = new();
    private List<ClassDto> _classes = new();

    public ClassControl(int roleId = 1)
    {
        _roleId = roleId;
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
            Text = "🏫  Quản lý Lớp học",
            Font = AppTheme.FontSubtitle, ForeColor = AppTheme.TextPrimary,
            Location = new Point(20, 16), AutoSize = true
        });

        if (_roleId is 1 or 2 or 3)
        {
            var btnAdd = AppTheme.MakePrimaryBtn("+ Mở lớp", 100, 34);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Click += (_, _) => ShowForm(null);
            head.Controls.Add(btnAdd);
            head.Resize += (_, _) => btnAdd.Location = new Point(head.Width - 120, 13);
        }

        // ── Toolbar ──────────────────────────────────────────────────────────
        var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(250, 250, 252) };
        bar.Paint += BorderBottom;
        _txtSearch = new TextBox
        {
            PlaceholderText = "Tìm lớp, khóa học, giáo viên...",
            Location = new Point(16, 12), Size = new Size(250, 28), Font = AppTheme.FontBody
        };
        _cmbStatus = new ComboBox
        {
            Location = new Point(278, 12), Size = new Size(150, 28),
            DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody
        };
        _cmbStatus.Items.AddRange(new object[] { "Tất cả", "Sắp khai giảng", "Đang học", "Hoàn thành", "Đã hủy" });
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
            new DataGridViewTextBoxColumn { HeaderText = "ID",         FillWeight = 6  },
            new DataGridViewTextBoxColumn { HeaderText = "Tên lớp",    FillWeight = 18 },
            new DataGridViewTextBoxColumn { HeaderText = "Khóa học",   FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Giáo viên",  FillWeight = 18 },
            new DataGridViewTextBoxColumn { HeaderText = "Sĩ số",      FillWeight = 9  },
            new DataGridViewTextBoxColumn { HeaderText = "Khai giảng", FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Kết thúc",   FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", FillWeight = 12 }
        );
        _grid.DoubleClick += (_, _) => EditSelected();

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("✏️  Sửa thông tin lớp", null, (_, _) => EditSelected());
        ctx.Items.Add("👥  Xem danh sách học viên", null, (_, _) => ViewStudents());
        ctx.Items.Add("📋  Xem chi tiết lớp", null, (_, _) => ViewDetail());
        if (_roleId is 1 or 2)
        {
            ctx.Items.Add("-");
            ctx.Items.Add("✅  Xác nhận khai giảng", null, async (_, _) => await ConfirmStartAsync());
            ctx.Items.Add("🚫  Hủy lớp học (yêu cầu mật khẩu)", null, async (_, _) => await CancelClassAsync());
        }
        _grid.ContextMenuStrip = ctx;

        _grid.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F5)  { _ = LoadAsync(); e.Handled = true; }
            if (e.KeyCode == Keys.F2)  { EditSelected(); e.Handled = true; }
            if (e.Control && e.KeyCode == Keys.N) { ShowForm(null); e.Handled = true; }
        };

        // ── Footer ──────────────────────────────────────────────────────────
        var foot = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(248, 249, 252) };
        _lblStatus = new Label
        {
            Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary,
            Text = "F5: Làm mới  |  F2: Sửa  |  Ctrl+N: Mở lớp mới  |  Double-click: Sửa  |  Chuột phải: Menu"
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
            _classes = await _service.GetAllAsync();
            Filter();
        }
        catch (Exception ex) { _lblStatus.Text = $"Lỗi: {ex.Message}"; }
        finally { Cursor = Cursors.Default; }
    }

    private void Filter()
    {
        var q = _txtSearch.Text.Trim().ToLowerInvariant();
        var list = _classes.Where(c =>
            string.IsNullOrEmpty(q) ||
            c.ClassName.ToLowerInvariant().Contains(q) ||
            c.CourseName.ToLowerInvariant().Contains(q) ||
            (c.MainTeacherName?.ToLowerInvariant().Contains(q) ?? false));

        list = _cmbStatus.SelectedIndex switch
        {
            1 => list.Where(c => c.Status == ClassStatus.Upcoming),
            2 => list.Where(c => c.Status == ClassStatus.Active),
            3 => list.Where(c => c.Status == ClassStatus.Completed),
            4 => list.Where(c => c.Status == ClassStatus.Cancelled),
            _ => list
        };

        RefreshGrid(list.ToList());
    }

    private void RefreshGrid(List<ClassDto> rows)
    {
        _grid.Rows.Clear();
        foreach (var c in rows)
        {
            _grid.Rows.Add(
                c.Id, c.ClassName, c.CourseName,
                c.MainTeacherName ?? "Chưa phân công",
                $"{c.EnrolledCount}/{c.MaxStudents}",
                c.StartDate.ToString("dd/MM/yyyy"),
                c.EndDate?.ToString("dd/MM/yyyy") ?? "—",
                StatusText(c.Status)
            );
            _grid.Rows[^1].Tag = c;

            // Tô màu row theo trạng thái
            var row = _grid.Rows[^1];
            row.DefaultCellStyle.ForeColor = c.Status switch
            {
                ClassStatus.Active    => AppTheme.Success,
                ClassStatus.Cancelled => AppTheme.Danger,
                ClassStatus.Completed => AppTheme.Info,
                _                     => AppTheme.TextPrimary
            };
        }
        _lblStatus.Text = $"Hiển thị {rows.Count} lớp học.  F5: Làm mới  |  F2: Sửa  |  Ctrl+N: Mở lớp mới";
    }

    private ClassDto? Selected() => _grid.CurrentRow?.Tag as ClassDto;

    private void EditSelected()
    {
        if (Selected() is ClassDto c) ShowForm(c);
    }

    private void ShowForm(ClassDto? cls)
    {
        using var form = new ClassFormDialog(_courses, cls);
        if (form.ShowDialog(this) != DialogResult.OK) return;

        if (cls == null)
        {
            var ok = _service.CreateAsync(form.Result!).GetAwaiter().GetResult();
            _lblStatus.Text = ok ? "✅ Đã tạo lớp học mới." : "❌ Tạo lớp thất bại. Kiểm tra dữ liệu.";
        }
        else
        {
            var ok = _service.UpdateAsync(cls.Id, new UpdateClassDto
            {
                ClassName   = form.Result!.ClassName,
                MaxStudents = form.Result.MaxStudents,
                StartDate   = form.Result.StartDate,
                EndDate     = form.Result.EndDate,
                Status      = cls.Status
            }).GetAwaiter().GetResult();
            _lblStatus.Text = ok ? "✅ Đã cập nhật lớp học." : "❌ Cập nhật thất bại.";
        }
        _ = LoadAsync();
    }

    private void ViewStudents()
    {
        if (Selected() is not ClassDto c) return;
        using var dlg = new ClassStudentsDialog(c, new EnrollmentService());
        dlg.ShowDialog(this);
    }

    private void ViewDetail()
    {
        if (Selected() is not ClassDto c) return;
        var info = $"Lớp: {c.ClassName}\nKhóa học: {c.CourseName}\n" +
                   $"Giáo viên: {c.MainTeacherName ?? "Chưa phân công"}\n" +
                   $"Sĩ số: {c.EnrolledCount}/{c.MaxStudents}\n" +
                   $"Khai giảng: {c.StartDate:dd/MM/yyyy}\n" +
                   $"Kết thúc: {c.EndDate?.ToString("dd/MM/yyyy") ?? "Chưa xác định"}\n" +
                   $"Trạng thái: {StatusText(c.Status)}";
        MessageBox.Show(info, $"Chi tiết: {c.ClassName}", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task ConfirmStartAsync()
    {
        if (Selected() is not ClassDto c) { _lblStatus.Text = "Hãy chọn một lớp trước."; return; }
        if (c.Status != ClassStatus.Upcoming) { _lblStatus.Text = "Chỉ có thể xác nhận khai giảng lớp đang 'Sắp khai giảng'."; return; }
        var (ok, msg) = await _service.ConfirmStartAsync(c.Id);
        _lblStatus.Text = ok ? $"✅ Đã xác nhận khai giảng lớp {c.ClassName}." : $"❌ {msg}";
        await LoadAsync();
    }

    private async Task CancelClassAsync()
    {
        if (Selected() is not ClassDto c) { _lblStatus.Text = "Hãy chọn một lớp trước."; return; }
        if (c.Status == ClassStatus.Cancelled) { _lblStatus.Text = "Lớp đã bị hủy rồi."; return; }
        if (!PasswordConfirmDialog.Confirm(this, $"Nhập mật khẩu để hủy lớp \"{c.ClassName}\".\nHành động này sẽ kích hoạt quy trình hoàn tiền!")) return;
        var (ok, msg) = await _service.CancelAsync(c.Id);
        _lblStatus.Text = ok ? $"✅ Đã hủy lớp {c.ClassName}. Đã kích hoạt quy trình hoàn tiền." : $"❌ {msg}";
        await LoadAsync();
    }

    private static string StatusText(ClassStatus s) => s switch
    {
        ClassStatus.Upcoming  => "Sắp khai giảng",
        ClassStatus.Active    => "Đang học",
        ClassStatus.Completed => "Hoàn thành",
        ClassStatus.Cancelled => "Đã hủy",
        _                     => s.ToString()
    };

    private static void BorderBottom(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}

// ── Dialog thêm / sửa lớp học ─────────────────────────────────────────────
internal sealed class ClassFormDialog : Form
{
    private readonly CourseService _courses;
    private readonly ClassDto? _existing;
    private TextBox _txtName = new();
    private NumericUpDown _numMax = new();
    private ComboBox _cmbCourse = new();
    private DateTimePicker _dtpStart = new();
    private DateTimePicker _dtpEnd = new();
    private CheckBox _chkEndDate = new();
    private Label _lblError = new();
    private List<CourseDto> _courseRows = new();
    public CreateClassDto? Result { get; private set; }

    public ClassFormDialog(CourseService courses, ClassDto? existing)
    {
        _courses = courses;
        _existing = existing;
        Text = existing == null ? "Mở lớp học mới" : "Sửa thông tin lớp";
        Size = new Size(480, 370);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.White;
        Build();
        if (!DesignModeHelper.IsInDesignMode)
            _ = LoadCoursesAsync();
    }

    private void Build()
    {
        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 7, ColumnCount = 2
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void Row(string lbl, Control ctrl, int r)
        {
            tbl.Controls.Add(new Label { Text = lbl, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, r);
            ctrl.Dock = DockStyle.Fill; ctrl.Font = AppTheme.FontBody;
            tbl.Controls.Add(ctrl, 1, r);
        }

        _cmbCourse = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        _numMax = new NumericUpDown { Minimum = 5, Maximum = 50, Value = 25, DecimalPlaces = 0 };
        _dtpStart = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(30) };
        _dtpEnd = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(120) };
        _chkEndDate = new CheckBox { Text = "Xác định ngày kết thúc", Font = AppTheme.FontBody, Checked = true };
        _chkEndDate.CheckedChanged += (_, _) => _dtpEnd.Enabled = _chkEndDate.Checked;

        Row("Khóa học *", _cmbCourse, 0);
        Row("Tên lớp *", _txtName = new TextBox(), 1);
        Row("Sĩ số tối đa", _numMax, 2);
        Row("Ngày khai giảng *", _dtpStart, 3);
        tbl.Controls.Add(_chkEndDate, 0, 4);
        tbl.SetColumnSpan(_chkEndDate, 2);
        _chkEndDate.Dock = DockStyle.Fill; _chkEndDate.Font = AppTheme.FontBody;
        Row("Ngày kết thúc", _dtpEnd, 5);

        _lblError = new Label { ForeColor = AppTheme.Danger, Font = AppTheme.FontSmall, AutoSize = true };
        tbl.Controls.Add(_lblError, 0, 6);
        tbl.SetColumnSpan(_lblError, 2);

        var foot = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 16, 0) };
        var btnSave   = AppTheme.MakePrimaryBtn("Lưu", 90, 34);
        var btnCancel = AppTheme.MakeOutlineBtn("Hủy", 90, 34);
        btnSave.Click   += (_, _) => Save();
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        foot.Controls.AddRange(new Control[] { btnSave, btnCancel });

        Controls.Add(tbl);
        Controls.Add(foot);
    }

    private async Task LoadCoursesAsync()
    {
        _courseRows = await _courses.GetAllAsync();
        _cmbCourse.DataSource    = _courseRows;
        _cmbCourse.DisplayMember = nameof(CourseDto.CourseName);
        _cmbCourse.ValueMember   = nameof(CourseDto.Id);

        if (_existing != null)
        {
            _txtName.Text   = _existing.ClassName;
            _numMax.Value   = Math.Max(5, Math.Min(50, _existing.MaxStudents));
            _dtpStart.Value = _existing.StartDate.ToDateTime(TimeOnly.MinValue);
            if (_existing.EndDate.HasValue)
            {
                _chkEndDate.Checked = true;
                _dtpEnd.Value = _existing.EndDate.Value.ToDateTime(TimeOnly.MinValue);
            }
            else { _chkEndDate.Checked = false; _dtpEnd.Enabled = false; }
            var matched = _courseRows.FirstOrDefault(c => c.Id == _existing.CourseId);
            if (matched != null) _cmbCourse.SelectedItem = matched;
        }
    }

    private void Save()
    {
        if (_cmbCourse.SelectedItem is not CourseDto course) { _lblError.Text = "Vui lòng chọn khóa học."; return; }
        if (string.IsNullOrWhiteSpace(_txtName.Text)) { _lblError.Text = "Tên lớp không được để trống."; return; }
        if (_dtpStart.Value.Date < DateTime.Today && _existing == null) { _lblError.Text = "Ngày khai giảng phải từ hôm nay trở đi."; return; }
        if (_chkEndDate.Checked && _dtpEnd.Value.Date <= _dtpStart.Value.Date) { _lblError.Text = "Ngày kết thúc phải sau ngày khai giảng."; return; }

        Result = new CreateClassDto
        {
            CourseId    = course.Id,
            ClassName   = _txtName.Text.Trim(),
            MaxStudents = (int)_numMax.Value,
            StartDate   = DateOnly.FromDateTime(_dtpStart.Value),
            EndDate     = _chkEndDate.Checked ? DateOnly.FromDateTime(_dtpEnd.Value) : null
        };
        DialogResult = DialogResult.OK;
    }
}

// ── Dialog xem danh sách học viên trong lớp ───────────────────────────────
internal sealed class ClassStudentsDialog : Form
{
    private readonly ClassDto _class;
    private readonly EnrollmentService _svc;
    private DataGridView _grid = new();
    private Label _status = new();

    public ClassStudentsDialog(ClassDto cls, EnrollmentService svc)
    {
        _class = cls; _svc = svc;
        Text = $"Danh sách học viên — {cls.ClassName}";
        Size = new Size(680, 460);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.White;
        Build();
        _ = LoadAsync();
    }

    private void Build()
    {
        _grid = AppTheme.MakeGrid();
        _grid.Dock = DockStyle.Fill;
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Mã HS",        FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Họ và tên",    FillWeight = 26 },
            new DataGridViewTextBoxColumn { HeaderText = "Ngày ĐK",      FillWeight = 13 },
            new DataGridViewTextBoxColumn { HeaderText = "Học phí",      FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Đã thu",       FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Còn lại",      FillWeight = 13 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái",   FillWeight = 14 }
        );

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.FromArgb(248, 249, 252) };
        _status = new Label { Location = new Point(16, 0), Size = new Size(440, 44), TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary };
        var btnClose = AppTheme.MakeOutlineBtn("Đóng", 90, 34);
        btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnClose.Click += (_, _) => Close();
        foot.Controls.AddRange(new Control[] { _status, btnClose });
        foot.Resize += (_, _) => btnClose.Location = new Point(foot.Width - 106, 5);

        Controls.Add(_grid);
        Controls.Add(foot);
    }

    private async Task LoadAsync()
    {
        try
        {
            var enrollments = await _svc.GetAllAsync(classId: _class.Id);
            _grid.Rows.Clear();
            foreach (var e in enrollments)
            {
                var remaining = e.TuitionFee - e.TotalPaid;
                _grid.Rows.Add(
                    e.StudentCode, e.StudentName,
                    e.EnrollmentDate.ToString("dd/MM/yyyy"),
                    $"{e.TuitionFee:N0}đ", $"{e.TotalPaid:N0}đ",
                    $"{remaining:N0}đ",
                    StatusLabel(e.Status)
                );
                // Tô màu nếu còn nợ
                if (remaining > 0) _grid.Rows[^1].DefaultCellStyle.BackColor = Color.FromArgb(255, 249, 235);
            }
            _status.Text = $"{enrollments.Count} học viên đã đăng ký.  Sĩ số hiện tại: {enrollments.Count}/{_class.MaxStudents}";
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
    }

    private static string StatusLabel(EnrollmentStatus s) => s switch
    {
        EnrollmentStatus.Active    => "Đang học",
        EnrollmentStatus.Pending   => "Chờ xác nhận",
        EnrollmentStatus.Completed => "Hoàn thành",
        EnrollmentStatus.Dropped   => "Đã nghỉ",
        _                          => s.ToString()
    };
}
