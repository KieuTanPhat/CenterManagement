using CenterManagement.Models.DTOs;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;
using CenterManagement.WinForms.Views.Enrollment;

namespace CenterManagement.WinForms.Views.Students;

public class StudentControl : UserControl
{
    private readonly StudentService    _service    = new();
    private readonly EnrollmentService _enrollSvc  = new();
    private readonly ClassService      _classSvc   = new();
    private DataGridView _dgv = new();
    private TextBox _txtSearch = new();
    private Label _lblStatus = new();
    private List<StudentDto> _students = new();

    public StudentControl()
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
        head.Paint += BorderBottom;
        head.Controls.Add(new Label
        {
            Text = "📚  Quản lý Học viên",
            Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(20, 16),
            AutoSize = true
        });
        // Nút "Đăng ký học mới" → mở form đăng ký (tạo học viên + ghi danh vào lớp)
        var btnAdd = AppTheme.MakePrimaryBtn("+ Đăng ký học mới", 160, 34);
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAdd.Click += (_, _) => OpenEnrollmentDialog();
        head.Controls.Add(btnAdd);
        head.Resize += (_, _) => btnAdd.Location = new Point(head.Width - 180, 13);

        var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(250, 250, 252) };
        bar.Paint += BorderBottom;
        _txtSearch = new TextBox
        {
            PlaceholderText = "Tìm theo tên, mã học sinh, SĐT, email...",
            Location = new Point(16, 12),
            Size = new Size(310, 28),
            Font = AppTheme.FontBody
        };
        var btnSearch = AppTheme.MakePrimaryBtn("Tìm", 80, 28);
        btnSearch.Location = new Point(338, 12);
        btnSearch.Click += async (_, _) => await SearchAsync();
        _txtSearch.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) await SearchAsync(); };
        bar.Controls.AddRange(new Control[] { _txtSearch, btnSearch });

        _dgv = AppTheme.MakeGrid();
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Mã HS", FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Họ và tên", FillWeight = 24 },
            new DataGridViewTextBoxColumn { HeaderText = "Ngày sinh", FillWeight = 11 },
            new DataGridViewTextBoxColumn { HeaderText = "Điện thoại", FillWeight = 13 },
            new DataGridViewTextBoxColumn { HeaderText = "Email", FillWeight = 18 },
            new DataGridViewTextBoxColumn { HeaderText = "Tên phụ huynh", FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "SĐT phụ huynh", FillWeight = 13 },
            new DataGridViewTextBoxColumn { HeaderText = "Ngày đăng ký", FillWeight = 11 }
        );
        _dgv.DoubleClick += (_, _) => EditSelected();

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("✏️ Sửa thông tin học viên",   null, (_, _) => EditSelected());
        ctx.Items.Add("📋 Xem lịch sử đăng ký",      null, (_, _) => ViewEnrollments());
        ctx.Items.Add("📝 Đăng ký thêm lớp học mới", null, (_, _) => OpenEnrollmentDialog());
        ctx.Items.Add("-");
        ctx.Items.Add("🔍 Xem chi tiết",              null, (_, _) => ViewDetail());
        _dgv.ContextMenuStrip = ctx;

        _dgv.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F5) { _ = LoadAsync(); e.Handled = true; }
            if (e.KeyCode == Keys.F2) { EditSelected(); e.Handled = true; }
            if (e.Control && e.KeyCode == Keys.N) { OpenEnrollmentDialog(); e.Handled = true; }
        };

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(248, 249, 252) };
        _lblStatus = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            Text = "F5: Làm mới  |  F2: Sửa  |  Ctrl+N: Đăng ký mới  |  Double-click: Sửa  |  Chuột phải: Menu"
        };
        foot.Controls.Add(_lblStatus);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        pnlGrid.Controls.Add(_dgv);

        card.Controls.Add(pnlGrid);
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
            _students = await _service.GetAllAsync();
            RefreshGrid(_students);
        }
        catch (Exception ex) { _lblStatus.Text = $"Lỗi: {ex.Message}"; }
        finally { Cursor = Cursors.Default; }
    }

    private async Task SearchAsync()
    {
        try
        {
            _students = await _service.GetAllAsync(_txtSearch.Text.Trim());
            RefreshGrid(_students);
        }
        catch (Exception ex) { _lblStatus.Text = $"Lỗi: {ex.Message}"; }
    }

    private void RefreshGrid(List<StudentDto> list)
    {
        _dgv.Rows.Clear();
        foreach (var s in list)
        {
            _dgv.Rows.Add(
                s.StudentCode,
                s.FullName,
                s.DateOfBirth?.ToString("dd/MM/yyyy") ?? "—",
                s.Phone ?? "—",
                s.Email ?? "—",
                s.ParentName ?? "—",
                s.ParentPhone ?? "—",
                s.CreatedAt.ToString("dd/MM/yyyy")
            );
            _dgv.Rows[^1].Tag = s;
        }
        _lblStatus.Text = $"Hiển thị {list.Count} học viên.  F5: Làm mới  |  F2: Sửa  |  Ctrl+N: Đăng ký mới";
    }

    private void EditSelected()
    {
        if (_dgv.CurrentRow?.Tag is StudentDto s) ShowEditForm(s);
    }

    private void OpenEnrollmentDialog()
    {
        // Thêm học viên = đăng ký vào lớp học (không thêm độc lập)
        using var dlg = new NewEnrollmentDialog(_service, _classSvc, _enrollSvc);
        if (dlg.ShowDialog(this) == DialogResult.OK) _ = LoadAsync();
    }

    private void ShowEditForm(StudentDto student)
    {
        using var form = new StudentFormDialog(student);
        if (form.ShowDialog(this) == DialogResult.OK) _ = LoadAsync();
    }

    private void ViewEnrollments()
    {
        if (_dgv.CurrentRow?.Tag is not StudentDto s) return;
        using var dlg = new StudentEnrollmentDialog(s, _service);
        dlg.ShowDialog(this);
    }

    private void ViewDetail()
    {
        if (_dgv.CurrentRow?.Tag is not StudentDto s) return;
        var info = $"Mã HS: {s.StudentCode}\nHọ tên: {s.FullName}\nNgày sinh: {s.DateOfBirth?.ToString("dd/MM/yyyy") ?? "—"}\n" +
                   $"Điện thoại: {s.Phone ?? "—"}\nEmail: {s.Email ?? "—"}\n" +
                   $"Phụ huynh: {s.ParentName ?? "—"} — SĐT: {s.ParentPhone ?? "—"}";
        MessageBox.Show(info, $"Chi tiết: {s.FullName}", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static void BorderBottom(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}

// ── Dialog lịch sử đăng ký học của học sinh ───────────────────────────────────
internal sealed class StudentEnrollmentDialog : Form
{
    private readonly StudentDto _student;
    private readonly StudentService _service;

    public StudentEnrollmentDialog(StudentDto student, StudentService service)
    {
        _student = student;
        _service = service;
        Text = $"Lịch sử đăng ký: {student.FullName}";
        Size = new Size(640, 420);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.White;
        Build();
        _ = LoadAsync();
    }

    private DataGridView _grid = new();
    private Label _status = new();

    private void Build()
    {
        _grid = AppTheme.MakeGrid();
        _grid.Dock = DockStyle.Fill;
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Lớp học", FillWeight = 24 },
            new DataGridViewTextBoxColumn { HeaderText = "Khóa học", FillWeight = 20 },
            new DataGridViewTextBoxColumn { HeaderText = "Ngày đăng ký", FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Ghi chú", FillWeight = 28 }
        );

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 44 };
        _status = new Label { Location = new Point(16, 0), Size = new Size(400, 44), TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary };
        var btn = AppTheme.MakeOutlineBtn("Đóng", 90, 34);
        btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btn.Click += (_, _) => Close();
        foot.Controls.AddRange(new Control[] { _status, btn });
        foot.Resize += (_, _) => btn.Location = new Point(foot.Width - 106, 5);

        Controls.Add(_grid);
        Controls.Add(foot);
    }

    private async Task LoadAsync()
    {
        try
        {
            var enrollments = await _service.GetEnrollmentsAsync(_student.Id);
            _grid.Rows.Clear();
            foreach (var e in enrollments)
                _grid.Rows.Add(e.ClassName, e.CourseName, e.EnrollmentDate.ToString("dd/MM/yyyy"), e.Status.ToString(), $"Đã đóng: {e.TotalPaid:N0}đ");
            _status.Text = $"{enrollments.Count} lượt đăng ký.";
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
    }
}

// ── Form thêm / sửa học sinh ─────────────────────────────────────────────────
public class StudentFormDialog : Form
{
    private readonly StudentDto? _existing;
    private readonly StudentService _service = new();
    private TextBox _txtName = new(), _txtPhone = new(), _txtEmail = new(), _txtParent = new(), _txtParentPhone = new();
    private DateTimePicker _dtpDob = new();
    private Label _lblError = new();

    public StudentFormDialog(StudentDto? existing)
    {
        _existing = existing;
        Text = existing == null ? "Thêm Học sinh" : "Sửa thông tin Học sinh";
        Size = new Size(460, 400);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.White;
        BuildForm();
        if (existing != null) Fill(existing);
    }

    private void BuildForm()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 8, ColumnCount = 2 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void AddRow(string label, Control ctrl, int row)
        {
            table.Controls.Add(new Label
            {
                Text = label, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill
            }, 0, row);
            ctrl.Dock = DockStyle.Fill;
            ctrl.Font = AppTheme.FontBody;
            table.Controls.Add(ctrl, 1, row);
        }

        _dtpDob = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddYears(-15) };
        AddRow("Họ và tên *", _txtName = new TextBox(), 0);
        AddRow("Ngày sinh", _dtpDob, 1);
        AddRow("Số điện thoại", _txtPhone = new TextBox(), 2);
        AddRow("Email", _txtEmail = new TextBox(), 3);
        AddRow("Tên phụ huynh", _txtParent = new TextBox(), 4);
        AddRow("SĐT phụ huynh", _txtParentPhone = new TextBox(), 5);

        _lblError = new Label { ForeColor = AppTheme.Danger, Font = AppTheme.FontSmall, AutoSize = true };
        table.Controls.Add(_lblError, 0, 6);
        table.SetColumnSpan(_lblError, 2);

        var foot = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 16, 0) };
        var btnSave = AppTheme.MakePrimaryBtn("Lưu", 90, 34);
        var btnCancel = AppTheme.MakeOutlineBtn("Hủy", 90, 34);
        btnSave.Click += async (_, _) => await SaveAsync();
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        foot.Controls.AddRange(new Control[] { btnSave, btnCancel });

        Controls.Add(table);
        Controls.Add(foot);
    }

    private void Fill(StudentDto s)
    {
        _txtName.Text = s.FullName;
        _txtPhone.Text = s.Phone ?? "";
        _txtEmail.Text = s.Email ?? "";
        _txtParent.Text = s.ParentName ?? "";
        _txtParentPhone.Text = s.ParentPhone ?? "";
        if (s.DateOfBirth.HasValue) _dtpDob.Value = s.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text)) { _lblError.Text = "Họ tên không được để trống."; return; }
        _lblError.Text = "";

        bool ok;
        static string? NE(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

        if (_existing == null)
        {
            ok = await _service.CreateAsync(new CreateStudentDto
            {
                FullName = _txtName.Text.Trim(),
                Phone = NE(_txtPhone.Text),
                Email = NE(_txtEmail.Text),
                DateOfBirth = DateOnly.FromDateTime(_dtpDob.Value),
                ParentName = NE(_txtParent.Text),
                ParentPhone = NE(_txtParentPhone.Text)
            });
        }
        else
        {
            ok = await _service.UpdateAsync(_existing.Id, new UpdateStudentDto
            {
                FullName = _txtName.Text.Trim(),
                Phone = NE(_txtPhone.Text),
                Email = NE(_txtEmail.Text),
                DateOfBirth = DateOnly.FromDateTime(_dtpDob.Value),
                ParentName = NE(_txtParent.Text),
                ParentPhone = NE(_txtParentPhone.Text)
            });
        }

        if (ok) DialogResult = DialogResult.OK;
        else _lblError.Text = "Lưu thất bại. Kiểm tra dữ liệu và thử lại.";
    }
}
