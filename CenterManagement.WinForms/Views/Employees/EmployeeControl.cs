using CenterManagement.Models.DTOs;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Employees;

public class EmployeeControl : UserControl
{
    private readonly EmployeeService _service = new();
    private readonly BranchService _branchService = new();
    private DataGridView _grid = new();
    private TextBox _search = new();
    private Label _status = new();
    private List<EmployeeDto> _employees = new();
    private List<BranchDto> _branches = new();

    public EmployeeControl()
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
            Text = "👤  Quản lý Nhân viên",
            Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(20, 16),
            AutoSize = true
        });
        var btnAdd = AppTheme.MakePrimaryBtn("+ Thêm nhân viên", 160, 34);
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAdd.Click += (_, _) => ShowForm(null);
        head.Controls.Add(btnAdd);
        head.Resize += (_, _) => btnAdd.Location = new Point(head.Width - 180, 13);

        var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(250, 250, 252) };
        bar.Paint += BorderBottom;
        _search = new TextBox
        {
            PlaceholderText = "Tìm theo tên, email, điện thoại...",
            Location = new Point(16, 12),
            Size = new Size(280, 28),
            Font = AppTheme.FontBody
        };
        var btnSearch = AppTheme.MakePrimaryBtn("Tìm", 75, 28);
        btnSearch.Location = new Point(308, 12);
        btnSearch.Click += async (_, _) => await SearchAsync();
        _search.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) await SearchAsync(); };
        bar.Controls.AddRange(new Control[] { _search, btnSearch });

        _grid = AppTheme.MakeGrid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", FillWeight = 5 },
            new DataGridViewTextBoxColumn { HeaderText = "Họ tên", FillWeight = 20 },
            new DataGridViewTextBoxColumn { HeaderText = "Vai trò", FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Chi nhánh", FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Chức vụ", FillWeight = 13 },
            new DataGridViewTextBoxColumn { HeaderText = "Phòng ban", FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Điện thoại", FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Ngày vào", FillWeight = 11 },
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
            Text = "F5: Làm mới  |  F2: Sửa  |  Ctrl+N: Thêm mới  |  Double-click: Sửa  |  Chuột phải: Menu"
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
            _branches = await _branchService.GetAllAsync();
            _employees = await _service.GetAllAsync();
            RefreshGrid(_employees);
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
        finally { Cursor = Cursors.Default; }
    }

    private async Task SearchAsync()
    {
        try
        {
            _employees = await _service.GetAllAsync(_search.Text.Trim());
            RefreshGrid(_employees);
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
    }

    private void RefreshGrid(List<EmployeeDto> list)
    {
        _grid.Rows.Clear();
        foreach (var e in list)
        {
            _grid.Rows.Add(
                e.Id,
                e.FullName,
                e.RoleName ?? (e.RoleId == 2 ? "Quản lý CN" : "Nhân viên"),
                e.BranchName ?? "—",
                e.Position ?? "—",
                e.Department ?? "—",
                e.Phone ?? "—",
                e.HireDate?.ToString("dd/MM/yyyy") ?? "—",
                e.IsActive ? "Đang làm" : "Nghỉ việc"
            );
            _grid.Rows[^1].Tag = e;
            if (!e.IsActive)
                _grid.Rows[^1].DefaultCellStyle.ForeColor = AppTheme.TextMuted;
        }
        _status.Text = $"Hiển thị {list.Count} nhân viên.  F5: Làm mới  |  F2: Sửa  |  Ctrl+N: Thêm";
    }

    private void EditSelected()
    {
        if (_grid.CurrentRow?.Tag is EmployeeDto e) ShowForm(e);
    }

    private void ViewDetail()
    {
        if (_grid.CurrentRow?.Tag is not EmployeeDto e) return;
        using var dlg = new EmployeeDetailDialog(e);
        dlg.ShowDialog(this);
    }

    private void ShowForm(EmployeeDto? employee)
    {
        using var form = new EmployeeFormDialog(employee, _service, _branches);
        if (form.ShowDialog(this) == DialogResult.OK)
            _ = LoadAsync();
    }

    private void ToggleActive()
    {
        if (_grid.CurrentRow?.Tag is not EmployeeDto e) return;
        var action = e.IsActive ? "khoá" : "mở khoá";
        if (!PasswordConfirmDialog.Confirm(this, $"Nhập mật khẩu để {action} tài khoản '{e.FullName}':")) return;
        _ = DoToggle(e);
    }

    private async Task DoToggle(EmployeeDto e)
    {
        var ok = await _service.ToggleActiveAsync(e.Id);
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

// ── Chi tiết nhân viên (read-only) ────────────────────────────────────────────
internal sealed class EmployeeDetailDialog : Form
{
    public EmployeeDetailDialog(EmployeeDto e)
    {
        Text = $"Chi tiết: {e.FullName}";
        Size = new Size(540, 560);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.White;

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };
        var table = new TableLayoutPanel
        {
            ColumnCount = 2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Width = 460, Padding = new Padding(0)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void AddRow(string label, string? value)
        {
            table.Controls.Add(new Label
            {
                Text = label,
                Font = AppTheme.FontBodyBold,
                ForeColor = AppTheme.TextSecondary,
                AutoSize = true,
                Padding = new Padding(0, 6, 0, 6)
            });
            table.Controls.Add(new Label
            {
                Text = value ?? "—",
                Font = AppTheme.FontBody,
                ForeColor = AppTheme.TextPrimary,
                AutoSize = true,
                Padding = new Padding(0, 6, 0, 6)
            });
        }

        AddRow("Họ tên:", e.FullName);
        AddRow("Vai trò:", e.RoleName ?? (e.RoleId == 2 ? "Quản lý chi nhánh" : "Nhân viên"));
        AddRow("Chi nhánh:", e.BranchName ?? "—");
        AddRow("Chức vụ:", e.Position);
        AddRow("Phòng ban:", e.Department);
        AddRow("Hợp đồng:", e.ContractType);
        AddRow("Ngày vào làm:", e.HireDate?.ToString("dd/MM/yyyy"));
        AddRow("HĐ từ:", e.ContractStartDate?.ToString("dd/MM/yyyy"));
        AddRow("HĐ đến:", e.ContractEndDate?.ToString("dd/MM/yyyy"));
        AddRow("Mức lương:", e.Salary.HasValue ? $"{e.Salary:N0} VNĐ" : "—");
        AddRow("Ngày sinh:", e.DateOfBirth?.ToString("dd/MM/yyyy"));
        AddRow("Giới tính:", e.Gender);
        AddRow("CCCD/CMND:", e.NationalId);
        AddRow("Địa chỉ:", e.Address);
        AddRow("Thành phố:", e.City);
        AddRow("Học vấn:", e.Education);
        AddRow("Chuyên ngành:", e.Major);
        AddRow("Liên hệ khẩn cấp:", e.EmergencyContact);
        AddRow("SĐT khẩn cấp:", e.EmergencyPhone);
        AddRow("Quan hệ:", e.EmergencyRelationship);
        AddRow("Tài khoản NH:", e.BankAccount);
        AddRow("Tên TK NH:", e.BankAccountName);
        AddRow("Ngân hàng:", e.BankName);
        AddRow("Mã số thuế:", e.TaxId);
        AddRow("Email:", e.Email);
        AddRow("Điện thoại:", e.Phone);
        AddRow("Trạng thái:", e.IsActive ? "Đang làm việc" : "Đã nghỉ việc");

        scroll.Controls.Add(table);
        table.Location = new Point(0, 0);

        var btnClose = AppTheme.MakeOutlineBtn("Đóng", 90, 34);
        btnClose.Click += (_, _) => Close();
        var foot = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.White };
        var fp = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 16, 0) };
        fp.Controls.Add(btnClose);
        foot.Controls.Add(fp);

        Controls.Add(scroll);
        Controls.Add(foot);
    }
}

// ── Form thêm / sửa nhân viên ─────────────────────────────────────────────────
internal sealed class EmployeeFormDialog : Form
{
    private readonly EmployeeDto? _existing;
    private readonly EmployeeService _service;
    private readonly List<BranchDto> _branches;

    // Thông tin cơ bản
    private readonly TextBox _txtUsername = new();
    private readonly TextBox _txtPassword = new();
    private readonly TextBox _txtFullName = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtPhone = new();
    private readonly ComboBox _cboRole = new();
    private readonly ComboBox _cboBranch = new();

    // Công việc
    private readonly TextBox _txtPosition = new();
    private readonly TextBox _txtDepartment = new();
    private readonly TextBox _txtContractType = new();
    private readonly DateTimePicker _dtpHire = new();
    private readonly DateTimePicker _dtpContractStart = new();
    private readonly DateTimePicker _dtpContractEnd = new();
    private readonly TextBox _txtSalary = new();

    // Cá nhân
    private readonly DateTimePicker _dtpDob = new();
    private readonly ComboBox _cboGender = new();
    private readonly TextBox _txtNationalId = new();
    private readonly TextBox _txtAddress = new();
    private readonly TextBox _txtCity = new();
    private readonly TextBox _txtEducation = new();
    private readonly TextBox _txtMajor = new();

    // Khẩn cấp
    private readonly TextBox _txtEmergencyContact = new();
    private readonly TextBox _txtEmergencyPhone = new();
    private readonly TextBox _txtEmergencyRelationship = new();

    // Ngân hàng / thuế
    private readonly TextBox _txtBankAccount = new();
    private readonly TextBox _txtBankAccountName = new();
    private readonly TextBox _txtBankName = new();
    private readonly TextBox _txtTaxId = new();

    private readonly Label _error = new();

    public EmployeeFormDialog(EmployeeDto? existing, EmployeeService service, List<BranchDto> branches)
    {
        _existing = existing;
        _service = service;
        _branches = branches;
        Text = existing == null ? "Thêm Nhân viên" : $"Sửa: {existing.FullName}";
        Size = new Size(620, 700);
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
            var lbl = new Label
            {
                Text = title,
                Font = AppTheme.FontBodyBold,
                ForeColor = AppTheme.Primary,
                Location = new Point(0, y),
                AutoSize = true
            };
            scroll.Controls.Add(lbl);
            y += 26;
            var sep = new Panel { Location = new Point(0, y), Size = new Size(560, 1), BackColor = AppTheme.Border };
            scroll.Controls.Add(sep);
            y += 8;
        }

        void Row(string label, Control ctrl)
        {
            scroll.Controls.Add(new Label
            {
                Text = label,
                Font = AppTheme.FontBody,
                ForeColor = AppTheme.TextSecondary,
                Location = new Point(0, y + 3),
                Size = new Size(160, 22)
            });
            ctrl.Location = new Point(165, y);
            ctrl.Size = new Size(360, 26);
            ctrl.Font = AppTheme.FontBody;
            scroll.Controls.Add(ctrl);
            y += 34;
        }

        // Role combobox
        _cboRole.Items.AddRange(new object[] { "Quản lý chi nhánh (Role 2)", "Nhân viên (Role 3)" });
        _cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboRole.SelectedIndex = 1;

        // Branch combobox
        _cboBranch.Items.Add("— Chưa xác định —");
        foreach (var b in _branches) _cboBranch.Items.Add(b);
        _cboBranch.DisplayMember = "BranchName";
        _cboBranch.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboBranch.SelectedIndex = 0;

        // Gender
        _cboGender.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
        _cboGender.DropDownStyle = ComboBoxStyle.DropDownList;

        // DatePicker defaults
        foreach (var dtp in new[] { _dtpHire, _dtpContractStart, _dtpContractEnd, _dtpDob })
        {
            dtp.Format = DateTimePickerFormat.Custom;
            dtp.CustomFormat = "dd/MM/yyyy";
            dtp.ShowCheckBox = true;
            dtp.Checked = false;
        }

        Section("THÔNG TIN TÀI KHOẢN");
        if (_existing == null) { Row("Tên đăng nhập *", _txtUsername); Row("Mật khẩu *", _txtPassword); }
        Row("Họ và tên *", _txtFullName);
        Row("Email *", _txtEmail);
        Row("Điện thoại", _txtPhone);
        Row("Vai trò *", _cboRole);
        Row("Chi nhánh", _cboBranch);

        Section("THÔNG TIN CÔNG VIỆC");
        Row("Chức vụ", _txtPosition);
        Row("Phòng ban", _txtDepartment);
        Row("Loại hợp đồng", _txtContractType);
        Row("Ngày vào làm", _dtpHire);
        Row("HĐ bắt đầu", _dtpContractStart);
        Row("HĐ kết thúc", _dtpContractEnd);
        Row("Mức lương (VNĐ)", _txtSalary);

        Section("THÔNG TIN CÁ NHÂN");
        Row("Ngày sinh", _dtpDob);
        Row("Giới tính", _cboGender);
        Row("CCCD/CMND", _txtNationalId);
        Row("Địa chỉ", _txtAddress);
        Row("Thành phố", _txtCity);
        Row("Học vấn", _txtEducation);
        Row("Chuyên ngành", _txtMajor);

        Section("LIÊN HỆ KHẨN CẤP");
        Row("Người liên hệ", _txtEmergencyContact);
        Row("SĐT khẩn cấp", _txtEmergencyPhone);
        Row("Quan hệ", _txtEmergencyRelationship);

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

    private void Fill(EmployeeDto e)
    {
        _txtFullName.Text = e.FullName;
        _txtEmail.Text = e.Email ?? "";
        _txtPhone.Text = e.Phone ?? "";
        _cboRole.SelectedIndex = e.RoleId == 2 ? 0 : 1;

        // Chọn chi nhánh
        if (e.BranchId.HasValue)
        {
            for (int i = 1; i < _cboBranch.Items.Count; i++)
            {
                if (_cboBranch.Items[i] is BranchDto b && b.Id == e.BranchId.Value)
                { _cboBranch.SelectedIndex = i; break; }
            }
        }

        _txtPosition.Text = e.Position ?? "";
        _txtDepartment.Text = e.Department ?? "";
        _txtContractType.Text = e.ContractType ?? "";
        SetDate(_dtpHire, e.HireDate);
        SetDate(_dtpContractStart, e.ContractStartDate);
        SetDate(_dtpContractEnd, e.ContractEndDate);
        _txtSalary.Text = e.Salary?.ToString("N0") ?? "";
        SetDate(_dtpDob, e.DateOfBirth);
        if (!string.IsNullOrEmpty(e.Gender))
        {
            var idx = _cboGender.Items.IndexOf(e.Gender);
            if (idx >= 0) _cboGender.SelectedIndex = idx;
        }
        _txtNationalId.Text = e.NationalId ?? "";
        _txtAddress.Text = e.Address ?? "";
        _txtCity.Text = e.City ?? "";
        _txtEducation.Text = e.Education ?? "";
        _txtMajor.Text = e.Major ?? "";
        _txtEmergencyContact.Text = e.EmergencyContact ?? "";
        _txtEmergencyPhone.Text = e.EmergencyPhone ?? "";
        _txtEmergencyRelationship.Text = e.EmergencyRelationship ?? "";
        _txtBankAccount.Text = e.BankAccount ?? "";
        _txtBankAccountName.Text = e.BankAccountName ?? "";
        _txtBankName.Text = e.BankName ?? "";
        _txtTaxId.Text = e.TaxId ?? "";
    }

    private static void SetDate(DateTimePicker dtp, DateOnly? d)
    {
        if (d.HasValue) { dtp.Value = d.Value.ToDateTime(TimeOnly.MinValue); dtp.Checked = true; }
        else dtp.Checked = false;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtFullName.Text) || string.IsNullOrWhiteSpace(_txtEmail.Text))
        { _error.Text = "Vui lòng điền đầy đủ thông tin bắt buộc (*)."; return; }

        if (_existing == null && (string.IsNullOrWhiteSpace(_txtUsername.Text) || string.IsNullOrWhiteSpace(_txtPassword.Text)))
        { _error.Text = "Tên đăng nhập và mật khẩu là bắt buộc."; return; }

        int roleId = _cboRole.SelectedIndex == 0 ? 2 : 3;
        int? branchId = _cboBranch.SelectedIndex > 0 && _cboBranch.SelectedItem is BranchDto b ? b.Id : null;
        decimal? salary = decimal.TryParse(_txtSalary.Text.Replace(",", "").Replace(".", ""), out var s) ? s : null;

        bool ok;
        if (_existing == null)
        {
            ok = await _service.CreateAsync(new CreateEmployeeDto
            {
                Username = _txtUsername.Text.Trim(),
                Password = _txtPassword.Text,
                FullName = _txtFullName.Text.Trim(),
                Email = _txtEmail.Text.Trim(),
                PhoneNumber = _txtPhone.Text.Trim().NullIfEmpty(),
                RoleId = roleId,
                BranchId = branchId,
                Position = _txtPosition.Text.Trim().NullIfEmpty(),
                Department = _txtDepartment.Text.Trim().NullIfEmpty(),
                ContractType = _txtContractType.Text.Trim().NullIfEmpty(),
                HireDate = _dtpHire.Checked ? DateOnly.FromDateTime(_dtpHire.Value) : null,
                ContractStartDate = _dtpContractStart.Checked ? DateOnly.FromDateTime(_dtpContractStart.Value) : null,
                ContractEndDate = _dtpContractEnd.Checked ? DateOnly.FromDateTime(_dtpContractEnd.Value) : null,
                Salary = salary,
                DateOfBirth = _dtpDob.Checked ? DateOnly.FromDateTime(_dtpDob.Value) : null,
                Gender = _cboGender.SelectedItem?.ToString().NullIfEmpty(),
                NationalId = _txtNationalId.Text.Trim().NullIfEmpty(),
                Address = _txtAddress.Text.Trim().NullIfEmpty(),
                City = _txtCity.Text.Trim().NullIfEmpty(),
                Education = _txtEducation.Text.Trim().NullIfEmpty(),
                Major = _txtMajor.Text.Trim().NullIfEmpty(),
                EmergencyContact = _txtEmergencyContact.Text.Trim().NullIfEmpty(),
                EmergencyPhone = _txtEmergencyPhone.Text.Trim().NullIfEmpty(),
                EmergencyRelationship = _txtEmergencyRelationship.Text.Trim().NullIfEmpty(),
                BankAccount = _txtBankAccount.Text.Trim().NullIfEmpty(),
                BankAccountName = _txtBankAccountName.Text.Trim().NullIfEmpty(),
                BankName = _txtBankName.Text.Trim().NullIfEmpty(),
                TaxId = _txtTaxId.Text.Trim().NullIfEmpty()
            });
        }
        else
        {
            ok = await _service.UpdateAsync(_existing.Id, new UpdateEmployeeDto
            {
                FullName = _txtFullName.Text.Trim(),
                PhoneNumber = _txtPhone.Text.Trim().NullIfEmpty(),
                BranchId = branchId,
                Position = _txtPosition.Text.Trim().NullIfEmpty(),
                Department = _txtDepartment.Text.Trim().NullIfEmpty(),
                ContractType = _txtContractType.Text.Trim().NullIfEmpty(),
                HireDate = _dtpHire.Checked ? DateOnly.FromDateTime(_dtpHire.Value) : null,
                ContractStartDate = _dtpContractStart.Checked ? DateOnly.FromDateTime(_dtpContractStart.Value) : null,
                ContractEndDate = _dtpContractEnd.Checked ? DateOnly.FromDateTime(_dtpContractEnd.Value) : null,
                Salary = salary,
                DateOfBirth = _dtpDob.Checked ? DateOnly.FromDateTime(_dtpDob.Value) : null,
                Gender = _cboGender.SelectedItem?.ToString().NullIfEmpty(),
                NationalId = _txtNationalId.Text.Trim().NullIfEmpty(),
                Address = _txtAddress.Text.Trim().NullIfEmpty(),
                City = _txtCity.Text.Trim().NullIfEmpty(),
                Education = _txtEducation.Text.Trim().NullIfEmpty(),
                Major = _txtMajor.Text.Trim().NullIfEmpty(),
                EmergencyContact = _txtEmergencyContact.Text.Trim().NullIfEmpty(),
                EmergencyPhone = _txtEmergencyPhone.Text.Trim().NullIfEmpty(),
                EmergencyRelationship = _txtEmergencyRelationship.Text.Trim().NullIfEmpty(),
                BankAccount = _txtBankAccount.Text.Trim().NullIfEmpty(),
                BankAccountName = _txtBankAccountName.Text.Trim().NullIfEmpty(),
                BankName = _txtBankName.Text.Trim().NullIfEmpty(),
                TaxId = _txtTaxId.Text.Trim().NullIfEmpty(),
                IsActive = _existing.IsActive
            });
        }

        if (ok) DialogResult = DialogResult.OK;
        else _error.Text = "Lưu thất bại. Kiểm tra lại dữ liệu hoặc kết nối máy chủ.";
    }
}

internal static class StringEx
{
    public static string? NullIfEmpty(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
