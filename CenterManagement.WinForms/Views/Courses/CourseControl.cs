using CenterManagement.Models.DTOs;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Courses;

public class CourseControl : UserControl
{
    private readonly CourseService _service = new();
    private DataGridView _dgv = new();
    private TextBox _txtSearch = new();
    private Label _lblStatus = new();
    private List<CourseDto> _courses = new();

    public CourseControl()
    {
        BuildUI();
        if (!DesignModeHelper.IsInDesignMode)
            _ = LoadAsync();
    }

    private void BuildUI()
    {
        BackColor = AppTheme.ContentBg;
        Dock = DockStyle.Fill;

        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        // Header
        var pnlHead = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        pnlHead.Paint += BorderBottom;
        var lblTitle = new Label
        {
            Text = "Quản lý Khóa học", Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true
        };
        var btnAdd = AppTheme.MakePrimaryBtn("+ Thêm mới", 120, 36);
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAdd.Click += (s, e) => ShowForm(null);
        pnlHead.Controls.AddRange(new Control[] { lblTitle, btnAdd });
        pnlHead.Resize += (s, e) => btnAdd.Location = new Point(pnlHead.Width - 140, 12);

        // Toolbar
        var pnlBar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(250, 250, 252) };
        pnlBar.Paint += BorderBottom;
        _txtSearch = new TextBox
        {
            PlaceholderText = "Tìm tên khóa học, mã khóa...",
            Location = new Point(16, 12), Size = new Size(300, 28), Font = AppTheme.FontBody
        };
        var btnSearch = AppTheme.MakePrimaryBtn("Tìm kiếm", 100, 28);
        btnSearch.Location = new Point(328, 12);
        btnSearch.Click += (s, e) => FilterGrid();
        _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) FilterGrid(); };
        pnlBar.Controls.AddRange(new Control[] { _txtSearch, btnSearch });

        // Grid
        _dgv = AppTheme.MakeGrid();
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Mã KH", FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Tên khóa học", FillWeight = 25 },
            new DataGridViewTextBoxColumn { HeaderText = "Mục tiêu", FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Học phí", FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Phí thi", FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Số tuần", FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Min/Max HS", FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", FillWeight = 12 }
        );
        var colEdit = new DataGridViewButtonColumn { HeaderText = "Sửa", Text = "Sửa", UseColumnTextForButtonValue = true, FillWeight = 7 };
        _dgv.Columns.Add(colEdit);
        _dgv.CellClick += OnCellClick;

        // Status bar
        var pnlStatus = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(248, 249, 252) };
        _lblStatus = new Label
        {
            Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary,
            Location = new Point(16, 0), Size = new Size(400, 30), TextAlign = ContentAlignment.MiddleLeft
        };
        pnlStatus.Controls.Add(_lblStatus);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        pnlGrid.Controls.Add(_dgv);

        card.Controls.Add(pnlGrid);
        card.Controls.Add(pnlStatus);
        card.Controls.Add(pnlBar);
        card.Controls.Add(pnlHead);
        Controls.Add(card);
    }

    private async Task LoadAsync()
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            _courses = await _service.GetAllAsync();
            if (InvokeRequired) Invoke(RefreshGrid);
            else RefreshGrid();
        }
        catch (Exception ex)
        {
            if (InvokeRequired) Invoke(() => _lblStatus.Text = $"Lỗi: {ex.Message}");
            else _lblStatus.Text = $"Lỗi: {ex.Message}";
        }
        finally { Cursor = Cursors.Default; }
    }

    private void FilterGrid()
    {
        var q = _txtSearch.Text.Trim().ToLower();
        var filtered = string.IsNullOrEmpty(q)
            ? _courses
            : _courses.Where(c => c.CourseName.ToLower().Contains(q) || (c.CourseCode?.ToLower().Contains(q) ?? false)).ToList();
        PopulateGrid(filtered);
    }

    private void RefreshGrid() => PopulateGrid(_courses);

    private void PopulateGrid(List<CourseDto> list)
    {
        _dgv.Rows.Clear();
        foreach (var c in list)
        {
            _dgv.Rows.Add(
                c.CourseCode ?? "—",
                c.CourseName,
                c.TargetScore ?? "—",
                c.TuitionFee.HasValue ? $"{c.TuitionFee.Value:N0} đ" : "—",
                c.ExamFee.HasValue ? $"{c.ExamFee.Value:N0} đ" : "—",
                c.DurationWeeks?.ToString() ?? "—",
                $"{c.MinStudents}/{c.MaxStudents}",
                c.IsActive ? "Đang mở" : "Đã đóng"
            );
            _dgv.Rows[_dgv.Rows.Count - 1].Tag = c;
        }
        _lblStatus.Text = $"Hiển thị {list.Count} khóa học";
    }

    private void OnCellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var editColIdx = _dgv.Columns.Count - 1;
        if (e.ColumnIndex == editColIdx && _dgv.Rows[e.RowIndex].Tag is CourseDto course)
            ShowForm(course);
    }

    private void ShowForm(CourseDto? course)
    {
        var form = new CourseFormDialog(course);
        if (form.ShowDialog(this) == DialogResult.OK)
            _ = LoadAsync();
    }

    private static void BorderBottom(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}

public class CourseFormDialog : Form
{
    private readonly CourseDto? _existing;
    private readonly CourseService _service = new();
    private TextBox _txtName = new(), _txtCode = new(), _txtTarget = new(), _txtDesc = new();
    private NumericUpDown _numFee = new(), _numExamFee = new(), _numWeeks = new(), _numMin = new(), _numMax = new();
    private CheckBox _chkActive = new();
    private Label _lblError = new();

    public CourseFormDialog(CourseDto? existing)
    {
        _existing = existing;
        Text = existing == null ? "Thêm Khóa học" : "Sửa Khóa học";
        Size = new Size(500, 580);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BuildForm();
        if (existing != null) Fill(existing);
    }

    private void BuildForm()
    {
        var pnl = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 12, ColumnCount = 2 };
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void AddRow(string label, Control ctrl, int row)
        {
            pnl.Controls.Add(new Label { Text = label, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
            ctrl.Dock = DockStyle.Fill;
            ctrl.Font = AppTheme.FontBody;
            pnl.Controls.Add(ctrl, 1, row);
        }

        _txtName = new TextBox(); _txtCode = new TextBox(); _txtTarget = new TextBox(); _txtDesc = new TextBox { Multiline = true, Height = 60 };
        _numFee = new NumericUpDown { Maximum = 100_000_000, DecimalPlaces = 0, ThousandsSeparator = true };
        _numExamFee = new NumericUpDown { Maximum = 10_000_000, DecimalPlaces = 0, ThousandsSeparator = true };
        _numWeeks = new NumericUpDown { Maximum = 52, Minimum = 1, Value = 12 };
        _numMin = new NumericUpDown { Maximum = 100, Minimum = 0, Value = 5 };
        _numMax = new NumericUpDown { Maximum = 100, Minimum = 1, Value = 30 };
        _chkActive = new CheckBox { Text = "Đang hoạt động", Checked = true };

        AddRow("Tên khóa học *", _txtName, 0);
        AddRow("Mã khóa", _txtCode, 1);
        AddRow("Mục tiêu đầu ra", _txtTarget, 2);
        AddRow("Học phí (VNĐ)", _numFee, 3);
        AddRow("Phí thi (VNĐ)", _numExamFee, 4);
        AddRow("Số tuần học", _numWeeks, 5);
        AddRow("Sĩ số tối thiểu", _numMin, 6);
        AddRow("Sĩ số tối đa", _numMax, 7);
        AddRow("Mô tả", _txtDesc, 8);
        pnl.Controls.Add(_chkActive, 1, 9);

        _lblError = new Label { ForeColor = AppTheme.Danger, Font = AppTheme.FontSmall, AutoSize = true };
        pnl.Controls.Add(_lblError, 0, 10); pnl.SetColumnSpan(_lblError, 2);

        var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.FromArgb(248, 249, 252) };
        var btnSave = AppTheme.MakePrimaryBtn("Lưu", 100, 36);
        var btnCancel = AppTheme.MakeOutlineBtn("Hủy", 90, 36);
        btnSave.Location = new Point(290, 8);
        btnCancel.Location = new Point(396, 8);
        btnSave.Click += async (s, e) => await SaveAsync();
        btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
        pnlBtn.Controls.AddRange(new Control[] { btnSave, btnCancel });

        Controls.Add(pnl);
        Controls.Add(pnlBtn);
    }

    private void Fill(CourseDto c)
    {
        _txtName.Text = c.CourseName;
        _txtCode.Text = c.CourseCode ?? "";
        _txtTarget.Text = c.TargetScore ?? "";
        _numFee.Value = c.TuitionFee.HasValue ? (decimal)c.TuitionFee.Value : 0;
        _numExamFee.Value = c.ExamFee.HasValue ? (decimal)c.ExamFee.Value : 0;
        _numWeeks.Value = c.DurationWeeks.HasValue ? Math.Min(c.DurationWeeks.Value, 52) : 12;
        _numMin.Value = c.MinStudents;
        _numMax.Value = c.MaxStudents;
        _chkActive.Checked = c.IsActive;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text)) { _lblError.Text = "Tên khóa học không được để trống."; return; }
        _lblError.Text = "";

        bool ok;
        if (_existing == null)
        {
            ok = await _service.CreateAsync(new CreateCourseDto
            {
                CourseName = _txtName.Text.Trim(),
                CourseCode = string.IsNullOrWhiteSpace(_txtCode.Text) ? null : _txtCode.Text.Trim(),
                TargetScore = string.IsNullOrWhiteSpace(_txtTarget.Text) ? null : _txtTarget.Text.Trim(),
                TuitionFee = _numFee.Value > 0 ? (decimal?)_numFee.Value : null,
                ExamFee = _numExamFee.Value > 0 ? (decimal?)_numExamFee.Value : null,
                DurationWeeks = (int)_numWeeks.Value,
                MinStudents = (int)_numMin.Value,
                MaxStudents = (int)_numMax.Value
            });
        }
        else
        {
            ok = await _service.UpdateAsync(_existing.Id, new UpdateCourseDto
            {
                CourseName = _txtName.Text.Trim(),
                CourseCode = string.IsNullOrWhiteSpace(_txtCode.Text) ? null : _txtCode.Text.Trim(),
                TargetScore = string.IsNullOrWhiteSpace(_txtTarget.Text) ? null : _txtTarget.Text.Trim(),
                TuitionFee = _numFee.Value > 0 ? (decimal?)_numFee.Value : null,
                ExamFee = _numExamFee.Value > 0 ? (decimal?)_numExamFee.Value : null,
                DurationWeeks = (int)_numWeeks.Value,
                MinStudents = (int)_numMin.Value,
                MaxStudents = (int)_numMax.Value,
                IsActive = _chkActive.Checked
            });
        }

        if (ok) DialogResult = DialogResult.OK;
        else _lblError.Text = "Lưu thất bại. Kiểm tra dữ liệu và thử lại.";
    }
}
