using CenterManagement.Models.DTOs;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Teachers;

public class TeacherManageControl : UserControl
{
    private readonly TeacherService _service = new();
    private DataGridView _grid = new();
    private TextBox _search = new();
    private Label _status = new();
    private List<TeacherDto> _teachers = new();

    public TeacherManageControl()
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
            Text = "👩‍🏫  Quản lý Giáo viên",
            Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(20, 16),
            AutoSize = true
        });
        var btnAdd = AppTheme.MakePrimaryBtn("+ Thêm giáo viên", 160, 34);
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAdd.Click += (_, _) => ShowForm(null);
        head.Controls.Add(btnAdd);
        head.Resize += (_, _) => btnAdd.Location = new Point(head.Width - 180, 13);

        var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(250, 250, 252) };
        bar.Paint += BorderBottom;
        _search = new TextBox
        {
            PlaceholderText = "Tìm theo tên, email, điện thoại, chuyên môn...",
            Location = new Point(16, 12),
            Size = new Size(300, 28),
            Font = AppTheme.FontBody
        };
        var btn = AppTheme.MakePrimaryBtn("Tìm", 75, 28);
        btn.Location = new Point(328, 12);
        btn.Click += (_, _) => Filter();
        _search.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) Filter(); };
        bar.Controls.AddRange(new Control[] { _search, btn });

        _grid = AppTheme.MakeGrid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", FillWeight = 5 },
            new DataGridViewTextBoxColumn { HeaderText = "Họ tên", FillWeight = 20 },
            new DataGridViewTextBoxColumn { HeaderText = "Email", FillWeight = 18 },
            new DataGridViewTextBoxColumn { HeaderText = "Điện thoại", FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Chuyên môn", FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Bằng cấp", FillWeight = 13 },
            new DataGridViewTextBoxColumn { HeaderText = "Kinh nghiệm", FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Lớp đang dạy", FillWeight = 9 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", FillWeight = 10 }
        );
        _grid.DoubleClick += (_, _) => EditSelected();

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("✏️ Sửa thông tin", null, (_, _) => EditSelected());
        ctx.Items.Add("🔍 Xem chi tiết", null, (_, _) => ViewDetail());
        ctx.Items.Add("-");
        ctx.Items.Add("🔒 Khoá / Mở tài khoản", null, (_, _) => ToggleActive());
        _grid.ContextMenuStrip = ctx;

        _grid.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F5) { _ = LoadAsync(); e.Handled = true; }
            if (e.KeyCode == Keys.F2) { EditSelected(); e.Handled = true; }
            if (e.Control && e.KeyCode == Keys.N) { ShowForm(null); e.Handled = true; }
        };

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(248, 249, 252) };
        _status = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            Text = "F5: Làm mới  |  F2: Sửa  |  Ctrl+N: Thêm  |  Chuột phải: Menu"
        };
        foot.Controls.Add(_status);

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
            _teachers = await _service.GetAllAsync();
            RefreshGrid(_teachers);
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
        finally { Cursor = Cursors.Default; }
    }

    private void Filter()
    {
        var q = _search.Text.Trim().ToLowerInvariant();
        var rows = string.IsNullOrEmpty(q)
            ? _teachers
            : _teachers.Where(t =>
                t.FullName.ToLowerInvariant().Contains(q) ||
                (t.Email?.ToLowerInvariant().Contains(q) ?? false) ||
                (t.Phone?.Contains(q) ?? false) ||
                (t.Specialization?.ToLowerInvariant().Contains(q) ?? false)).ToList();
        RefreshGrid(rows);
    }

    private void RefreshGrid(List<TeacherDto> rows)
    {
        _grid.Rows.Clear();
        foreach (var t in rows)
        {
            var exp = t.YearsOfExperience.HasValue ? $"{t.YearsOfExperience} năm" : "—";
            _grid.Rows.Add(t.Id, t.FullName, t.Email ?? "—", t.Phone ?? "—",
                t.Specialization ?? "—", t.Qualification ?? "—", exp,
                t.ActiveClassCount, t.IsActive ? "Đang dạy" : "Tạm ngưng");
            _grid.Rows[^1].Tag = t;
            if (!t.IsActive) _grid.Rows[^1].DefaultCellStyle.ForeColor = AppTheme.TextMuted;
        }
        _status.Text = $"Hiển thị {rows.Count} giáo viên.  F5: Làm mới  |  F2: Sửa  |  Ctrl+N: Thêm";
    }

    private void EditSelected()
    {
        if (_grid.CurrentRow?.Tag is TeacherDto t) ShowForm(t);
    }

    private void ShowForm(TeacherDto? teacher)
    {
        using var form = new TeacherFormDialog(teacher, _service);
        if (form.ShowDialog(this) == DialogResult.OK)
            _ = LoadAsync();
    }

    private void ViewDetail()
    {
        if (_grid.CurrentRow?.Tag is not TeacherDto t) return;
        using var dlg = new TeacherDetailDialog(t);
        dlg.ShowDialog(this);
    }

    private void ToggleActive()
    {
        if (_grid.CurrentRow?.Tag is not TeacherDto t) return;
        var action = t.IsActive ? "khoá" : "mở khoá";
        if (!PasswordConfirmDialog.Confirm(this, $"Nhập mật khẩu để {action} tài khoản giáo viên '{t.FullName}':")) return;
        _ = DoToggle(t.Id);
    }

    private async Task DoToggle(int id)
    {
        var ok = await _service.ToggleActiveAsync(id);
        if (ok) await LoadAsync();
        else MessageBox.Show("Thao tác thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static void BorderBottom(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}

// ── Chi tiết giáo viên (read-only) ───────────────────────────────────────────
internal sealed class TeacherDetailDialog : Form
{
    public TeacherDetailDialog(TeacherDto t)
    {
        Text = $"Chi tiết: {t.FullName}";
        Size = new Size(520, 540);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.White;

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };
        var table = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Width = 440 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void AddRow(string label, string? value)
        {
            table.Controls.Add(new Label { Text = label, Font = AppTheme.FontBodyBold, ForeColor = AppTheme.TextSecondary, AutoSize = true, Padding = new Padding(0, 5, 0, 5) });
            table.Controls.Add(new Label { Text = value ?? "—", Font = AppTheme.FontBody, ForeColor = AppTheme.TextPrimary, AutoSize = true, Padding = new Padding(0, 5, 0, 5) });
        }

        AddRow("Họ tên:", t.FullName);
        AddRow("Email:", t.Email);
        AddRow("Điện thoại:", t.Phone);
        AddRow("Chuyên môn:", t.Specialization);
        AddRow("Bằng cấp:", t.Qualification);
        AddRow("Kinh nghiệm:", t.YearsOfExperience.HasValue ? $"{t.YearsOfExperience} năm" : null);
        AddRow("Chứng chỉ:", t.Certificates);
        AddRow("Giới thiệu:", t.Biography);
        AddRow("Ngày sinh:", t.DateOfBirth?.ToString("dd/MM/yyyy"));
        AddRow("Giới tính:", t.Gender);
        AddRow("CCCD/CMND:", t.NationalId);
        AddRow("Địa chỉ:", t.Address);
        AddRow("Thành phố:", t.City);
        AddRow("Loại HĐ:", t.ContractType);
        AddRow("HĐ bắt đầu:", t.ContractStartDate?.ToString("dd/MM/yyyy"));
        AddRow("HĐ kết thúc:", t.ContractEndDate?.ToString("dd/MM/yyyy"));
        AddRow("Tài khoản NH:", t.BankAccount);
        AddRow("Tên TK:", t.BankAccountName);
        AddRow("Ngân hàng:", t.BankName);
        AddRow("Mã số thuế:", t.TaxId);
        AddRow("Lớp đang dạy:", $"{t.ActiveClassCount} lớp");
        AddRow("Trạng thái:", t.IsActive ? "Đang hoạt động" : "Tạm ngưng");

        scroll.Controls.Add(table);
        table.Location = new Point(0, 0);

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.White };
        var fp = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 16, 0) };
        var btn = AppTheme.MakeOutlineBtn("Đóng", 90, 34);
        btn.Click += (_, _) => Close();
        fp.Controls.Add(btn);
        foot.Controls.Add(fp);

        Controls.Add(scroll);
        Controls.Add(foot);
    }
}

// ── Form thêm / sửa giáo viên ────────────────────────────────────────────────
internal sealed class TeacherFormDialog : Form
{
    private readonly TeacherDto? _existing;
    private readonly TeacherService _service;

    private readonly TextBox _txtUsername = new();
    private readonly TextBox _txtPassword = new();
    private readonly TextBox _txtFullName = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtPhone = new();
    private readonly TextBox _txtSpecialization = new();
    private readonly TextBox _txtQualification = new();
    private readonly NumericUpDown _numExp = new();
    private readonly TextBox _txtCertificates = new();
    private readonly TextBox _txtBiography = new();
    private readonly DateTimePicker _dtpDob = new();
    private readonly ComboBox _cboGender = new();
    private readonly TextBox _txtNationalId = new();
    private readonly TextBox _txtAddress = new();
    private readonly TextBox _txtCity = new();
    private readonly TextBox _txtContractType = new();
    private readonly DateTimePicker _dtpContractStart = new();
    private readonly DateTimePicker _dtpContractEnd = new();
    private readonly TextBox _txtBankAccount = new();
    private readonly TextBox _txtBankAccountName = new();
    private readonly TextBox _txtBankName = new();
    private readonly TextBox _txtTaxId = new();

    private readonly Label _error = new();

    public TeacherFormDialog(TeacherDto? existing, TeacherService service)
    {
        _existing = existing;
        _service = service;
        Text = existing == null ? "Thêm Giáo viên" : $"Sửa: {existing.FullName}";
        Size = new Size(600, 680);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Build();
        if (existing != null) Fill(existing);
    }

    private void Build()
    {
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(16) };
        var y = 0;

        void Section(string title)
        {
            scroll.Controls.Add(new Label { Text = title, Font = AppTheme.FontBodyBold, ForeColor = AppTheme.Primary, Location = new Point(0, y), AutoSize = true });
            y += 26;
            scroll.Controls.Add(new Panel { Location = new Point(0, y), Size = new Size(540, 1), BackColor = AppTheme.Border });
            y += 8;
        }

        void Row(string label, Control ctrl)
        {
            scroll.Controls.Add(new Label { Text = label, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary, Location = new Point(0, y + 3), Size = new Size(160, 22) });
            ctrl.Location = new Point(165, y);
            ctrl.Size = new Size(350, 26);
            ctrl.Font = AppTheme.FontBody;
            scroll.Controls.Add(ctrl);
            y += 34;
        }

        _cboGender.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
        _cboGender.DropDownStyle = ComboBoxStyle.DropDownList;
        _numExp.Minimum = 0; _numExp.Maximum = 50;

        foreach (var dtp in new[] { _dtpDob, _dtpContractStart, _dtpContractEnd })
        {
            dtp.Format = DateTimePickerFormat.Custom;
            dtp.CustomFormat = "dd/MM/yyyy";
            dtp.ShowCheckBox = true;
            dtp.Checked = false;
        }

        Section("THÔNG TIN TÀI KHOẢN");
        if (_existing == null) { Row("Tên đăng nhập *", _txtUsername); Row("Mật khẩu *", _txtPassword); _txtPassword.UseSystemPasswordChar = true; }
        Row("Họ và tên *", _txtFullName);
        Row("Email *", _txtEmail);
        Row("Điện thoại", _txtPhone);

        Section("CHUYÊN MÔN & KINH NGHIỆM");
        Row("Chuyên môn", _txtSpecialization);
        Row("Bằng cấp", _txtQualification);
        Row("Kinh nghiệm (năm)", _numExp);
        Row("Chứng chỉ", _txtCertificates);
        Row("Giới thiệu", _txtBiography);

        Section("THÔNG TIN CÁ NHÂN");
        Row("Ngày sinh", _dtpDob);
        Row("Giới tính", _cboGender);
        Row("CCCD/CMND", _txtNationalId);
        Row("Địa chỉ", _txtAddress);
        Row("Thành phố", _txtCity);

        Section("HỢP ĐỒNG");
        Row("Loại hợp đồng", _txtContractType);
        Row("HĐ bắt đầu", _dtpContractStart);
        Row("HĐ kết thúc", _dtpContractEnd);

        Section("NGÂN HÀNG & THUẾ");
        Row("Số tài khoản", _txtBankAccount);
        Row("Tên chủ TK", _txtBankAccountName);
        Row("Ngân hàng", _txtBankName);
        Row("Mã số thuế", _txtTaxId);

        _error.ForeColor = AppTheme.Danger;
        _error.Font = AppTheme.FontSmall;
        _error.AutoSize = true;
        _error.Location = new Point(0, y);
        scroll.Controls.Add(_error);
        y += 30;
        scroll.AutoScrollMinSize = new Size(0, y + 10);

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.White };
        foot.Paint += (s, e) => { using var p = new Pen(AppTheme.Border); e.Graphics.DrawLine(p, 0, 0, ((Control)s!).Width, 0); };
        var fp = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 16, 0) };
        var save = AppTheme.MakePrimaryBtn("Lưu", 90, 34);
        var cancel = AppTheme.MakeOutlineBtn("Hủy", 90, 34);
        save.Click += async (_, _) => await SaveAsync();
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        fp.Controls.AddRange(new Control[] { save, cancel });
        foot.Controls.Add(fp);

        Controls.Add(scroll);
        Controls.Add(foot);
    }

    private void Fill(TeacherDto t)
    {
        _txtFullName.Text = t.FullName;
        _txtEmail.Text = t.Email ?? "";
        _txtPhone.Text = t.Phone ?? "";
        _txtSpecialization.Text = t.Specialization ?? "";
        _txtQualification.Text = t.Qualification ?? "";
        _numExp.Value = t.YearsOfExperience ?? 0;
        _txtCertificates.Text = t.Certificates ?? "";
        _txtBiography.Text = t.Biography ?? "";
        SetDate(_dtpDob, t.DateOfBirth);
        if (!string.IsNullOrEmpty(t.Gender)) { var i = _cboGender.Items.IndexOf(t.Gender); if (i >= 0) _cboGender.SelectedIndex = i; }
        _txtNationalId.Text = t.NationalId ?? "";
        _txtAddress.Text = t.Address ?? "";
        _txtCity.Text = t.City ?? "";
        _txtContractType.Text = t.ContractType ?? "";
        SetDate(_dtpContractStart, t.ContractStartDate);
        SetDate(_dtpContractEnd, t.ContractEndDate);
        _txtBankAccount.Text = t.BankAccount ?? "";
        _txtBankAccountName.Text = t.BankAccountName ?? "";
        _txtBankName.Text = t.BankName ?? "";
        _txtTaxId.Text = t.TaxId ?? "";
    }

    private static void SetDate(DateTimePicker dtp, DateOnly? d)
    {
        if (d.HasValue) { dtp.Value = d.Value.ToDateTime(TimeOnly.MinValue); dtp.Checked = true; }
        else dtp.Checked = false;
    }

    private static string? NE(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtFullName.Text) || string.IsNullOrWhiteSpace(_txtEmail.Text))
        { _error.Text = "Vui lòng điền Họ tên và Email."; return; }
        if (_existing == null && (string.IsNullOrWhiteSpace(_txtUsername.Text) || string.IsNullOrWhiteSpace(_txtPassword.Text)))
        { _error.Text = "Tên đăng nhập và mật khẩu là bắt buộc."; return; }

        bool ok;
        if (_existing == null)
        {
            ok = await _service.CreateAsync(new CreateTeacherDto
            {
                Username = _txtUsername.Text.Trim(),
                Password = _txtPassword.Text,
                FullName = _txtFullName.Text.Trim(),
                Email = _txtEmail.Text.Trim(),
                PhoneNumber = NE(_txtPhone.Text),
                Specialization = NE(_txtSpecialization.Text),
                Qualification = NE(_txtQualification.Text),
                YearsOfExperience = _numExp.Value > 0 ? (int)_numExp.Value : null,
                Certificates = NE(_txtCertificates.Text),
                Biography = NE(_txtBiography.Text),
                DateOfBirth = _dtpDob.Checked ? DateOnly.FromDateTime(_dtpDob.Value) : null,
                Gender = NE(_cboGender.SelectedItem?.ToString()),
                NationalId = NE(_txtNationalId.Text),
                Address = NE(_txtAddress.Text),
                City = NE(_txtCity.Text),
                ContractType = NE(_txtContractType.Text),
                ContractStartDate = _dtpContractStart.Checked ? DateOnly.FromDateTime(_dtpContractStart.Value) : null,
                ContractEndDate = _dtpContractEnd.Checked ? DateOnly.FromDateTime(_dtpContractEnd.Value) : null,
                BankAccount = NE(_txtBankAccount.Text),
                BankAccountName = NE(_txtBankAccountName.Text),
                BankName = NE(_txtBankName.Text),
                TaxId = NE(_txtTaxId.Text)
            });
        }
        else
        {
            ok = await _service.UpdateAsync(_existing.Id, new UpdateTeacherDto
            {
                FullName = _txtFullName.Text.Trim(),
                PhoneNumber = NE(_txtPhone.Text),
                Specialization = NE(_txtSpecialization.Text),
                Qualification = NE(_txtQualification.Text),
                YearsOfExperience = _numExp.Value > 0 ? (int)_numExp.Value : null,
                Certificates = NE(_txtCertificates.Text),
                Biography = NE(_txtBiography.Text),
                DateOfBirth = _dtpDob.Checked ? DateOnly.FromDateTime(_dtpDob.Value) : null,
                Gender = NE(_cboGender.SelectedItem?.ToString()),
                NationalId = NE(_txtNationalId.Text),
                Address = NE(_txtAddress.Text),
                City = NE(_txtCity.Text),
                ContractType = NE(_txtContractType.Text),
                ContractStartDate = _dtpContractStart.Checked ? DateOnly.FromDateTime(_dtpContractStart.Value) : null,
                ContractEndDate = _dtpContractEnd.Checked ? DateOnly.FromDateTime(_dtpContractEnd.Value) : null,
                BankAccount = NE(_txtBankAccount.Text),
                BankAccountName = NE(_txtBankAccountName.Text),
                BankName = NE(_txtBankName.Text),
                TaxId = NE(_txtTaxId.Text),
                IsActive = _existing.IsActive
            });
        }

        if (ok) DialogResult = DialogResult.OK;
        else _error.Text = "Lưu thất bại. Kiểm tra dữ liệu hoặc kết nối máy chủ.";
    }
}
