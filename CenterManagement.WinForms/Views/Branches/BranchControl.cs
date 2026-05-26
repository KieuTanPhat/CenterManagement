using CenterManagement.Models.DTOs;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Branches;

public class BranchControl : UserControl
{
    private readonly BranchService _service = new();
    private DataGridView _grid = new();
    private Label _status = new();
    private List<BranchDto> _branches = new();

    public BranchControl()
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
        head.Controls.Add(new Label { Text = "🏢  Quản lý Chi nhánh", Font = AppTheme.FontSubtitle, ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true });
        var btnAdd = AppTheme.MakePrimaryBtn("+ Thêm chi nhánh", 150, 34);
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAdd.Click += (_, _) => ShowForm(null);
        head.Controls.Add(btnAdd);
        head.Resize += (_, _) => btnAdd.Location = new Point(head.Width - 170, 13);

        _grid = AppTheme.MakeGrid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", FillWeight = 7 },
            new DataGridViewTextBoxColumn { HeaderText = "Tên chi nhánh", FillWeight = 26 },
            new DataGridViewTextBoxColumn { HeaderText = "Thành phố", FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Địa chỉ", FillWeight = 28 },
            new DataGridViewTextBoxColumn { HeaderText = "Điện thoại", FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", FillWeight = 11 }
        );
        _grid.DoubleClick += (_, _) => EditSelected();

        var ctxMenu = new ContextMenuStrip();
        ctxMenu.Items.Add("✏️ Sửa chi nhánh", null, (_, _) => EditSelected());
        ctxMenu.Items.Add("-");
        ctxMenu.Items.Add("❌ Vô hiệu hóa", null, (_, _) => DeactivateSelected());
        _grid.ContextMenuStrip = ctxMenu;

        // F5 = reload, F2 = edit
        _grid.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F5) { _ = LoadAsync(); e.Handled = true; }
            if (e.KeyCode == Keys.F2) { EditSelected(); e.Handled = true; }
        };

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(248, 249, 252) };
        _status = new Label { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary };
        _status.Text = "F5: Làm mới  |  F2: Sửa  |  Double-click: Sửa  |  Chuột phải: Menu";
        foot.Controls.Add(_status);

        card.Controls.Add(_grid);
        card.Controls.Add(foot);
        card.Controls.Add(head);
        Controls.Add(card);
    }

    private async Task LoadAsync()
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            _branches = await _service.GetAllAsync();
            _grid.Rows.Clear();
            foreach (var b in _branches)
            {
                _grid.Rows.Add(b.Id, b.BranchName, b.City, b.Address, b.Phone, b.IsActive ? "Hoạt động" : "Đã đóng");
                _grid.Rows[^1].Tag = b;
                if (!b.IsActive) _grid.Rows[^1].DefaultCellStyle.ForeColor = AppTheme.TextMuted;
            }
            _status.Text = $"Hiển thị {_branches.Count} chi nhánh.  F5: Làm mới  |  F2 / Double-click: Sửa";
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
        finally { Cursor = Cursors.Default; }
    }

    private void EditSelected()
    {
        if (_grid.CurrentRow?.Tag is BranchDto b) ShowForm(b);
    }

    private void ShowForm(BranchDto? branch)
    {
        using var form = new BranchFormDialog(branch, _service);
        if (form.ShowDialog(this) == DialogResult.OK)
            _ = LoadAsync();
    }

    private void DeactivateSelected()
    {
        if (_grid.CurrentRow?.Tag is not BranchDto b) return;
        if (!PasswordConfirmDialog.Confirm(this, $"Nhập mật khẩu để vô hiệu hóa chi nhánh '{b.BranchName}':")) return;
        MessageBox.Show("Tính năng vô hiệu hóa đang được phát triển.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static void BorderBottom(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}

internal sealed class BranchFormDialog : Form
{
    private readonly BranchDto? _existing;
    private readonly BranchService _service;
    private readonly TextBox _txtName = new(), _txtCity = new(), _txtAddress = new(), _txtPhone = new();
    private readonly Label _error = new();

    public BranchFormDialog(BranchDto? existing, BranchService service)
    {
        _existing = existing;
        _service = service;
        Text = existing == null ? "Thêm Chi nhánh" : "Sửa Chi nhánh";
        Size = new Size(460, 300);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Build();
        if (existing != null) Fill(existing);
    }

    private void Build()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 6 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(table, "Tên chi nhánh *", _txtName, 0);
        AddRow(table, "Thành phố *", _txtCity, 1);
        AddRow(table, "Địa chỉ *", _txtAddress, 2);
        AddRow(table, "Điện thoại *", _txtPhone, 3);
        _error.ForeColor = AppTheme.Danger; _error.Font = AppTheme.FontSmall; _error.AutoSize = true;
        table.Controls.Add(_error, 0, 4); table.SetColumnSpan(_error, 2);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 16, 0) };
        var save = AppTheme.MakePrimaryBtn("Lưu", 90, 34);
        var cancel = AppTheme.MakeOutlineBtn("Hủy", 90, 34);
        save.Click += async (_, _) => await SaveAsync();
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.AddRange(new Control[] { save, cancel });
        Controls.Add(table);
        Controls.Add(buttons);
    }

    private static void AddRow(TableLayoutPanel t, string label, Control ctrl, int row)
    {
        t.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontBody }, 0, row);
        ctrl.Dock = DockStyle.Fill; ctrl.Font = AppTheme.FontBody;
        t.Controls.Add(ctrl, 1, row);
    }

    private void Fill(BranchDto b) { _txtName.Text = b.BranchName; _txtCity.Text = b.City; _txtAddress.Text = b.Address; _txtPhone.Text = b.Phone; }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text) || string.IsNullOrWhiteSpace(_txtCity.Text) ||
            string.IsNullOrWhiteSpace(_txtAddress.Text) || string.IsNullOrWhiteSpace(_txtPhone.Text))
        { _error.Text = "Vui lòng điền đầy đủ thông tin bắt buộc."; return; }

        bool ok;
        if (_existing == null)
        {
            ok = await _service.CreateBranchAsync(new CreateBranchDto
            {
                BranchName = _txtName.Text.Trim(), City = _txtCity.Text.Trim(),
                Address = _txtAddress.Text.Trim(), Phone = _txtPhone.Text.Trim()
            });
        }
        else
        {
            ok = await _service.UpdateBranchAsync(_existing.Id, new UpdateBranchDto
            {
                BranchName = _txtName.Text.Trim(), City = _txtCity.Text.Trim(),
                Address = _txtAddress.Text.Trim(), Phone = _txtPhone.Text.Trim(), IsActive = true
            });
        }
        if (ok) DialogResult = DialogResult.OK;
        else _error.Text = "Lưu thất bại. Kiểm tra dữ liệu.";
    }
}
