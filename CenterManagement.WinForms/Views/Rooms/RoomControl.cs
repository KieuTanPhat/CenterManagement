using CenterManagement.Models.DTOs;
using CenterManagement.Models.Enums;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Rooms;

public class RoomControl : UserControl
{
    private readonly BranchService _service = new();
    private DataGridView _grid = new();
    private ComboBox _cboBranch = new();
    private Label _status = new();
    private List<RoomDto> _rooms = new();
    private List<BranchDto> _branches = new();

    public RoomControl()
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
            Text = "🚪  Quản lý Phòng học",
            Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(20, 16),
            AutoSize = true
        });
        var btnAdd = AppTheme.MakePrimaryBtn("+ Thêm phòng", 140, 34);
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAdd.Click += (_, _) => ShowForm(null);
        head.Controls.Add(btnAdd);
        head.Resize += (_, _) => btnAdd.Location = new Point(head.Width - 160, 13);

        var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(250, 250, 252) };
        bar.Paint += BorderBottom;
        var lblFilter = new Label
        {
            Text = "Chi nhánh:",
            Location = new Point(16, 16),
            AutoSize = true,
            Font = AppTheme.FontBody,
            ForeColor = AppTheme.TextSecondary
        };
        _cboBranch = new ComboBox
        {
            Location = new Point(90, 12),
            Size = new Size(220, 28),
            Font = AppTheme.FontBody,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cboBranch.SelectedIndexChanged += async (_, _) => await FilterByBranch();
        bar.Controls.AddRange(new Control[] { lblFilter, _cboBranch });

        _grid = AppTheme.MakeGrid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", FillWeight = 6 },
            new DataGridViewTextBoxColumn { HeaderText = "Tên phòng", FillWeight = 22 },
            new DataGridViewTextBoxColumn { HeaderText = "Chi nhánh", FillWeight = 22 },
            new DataGridViewTextBoxColumn { HeaderText = "Loại phòng", FillWeight = 18 },
            new DataGridViewTextBoxColumn { HeaderText = "Sức chứa", FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", FillWeight = 18 }
        );
        _grid.DoubleClick += (_, _) => EditSelected();

        var ctx = new ContextMenuStrip();
        ctx.Items.Add("✏️ Sửa phòng", null, (_, _) => EditSelected());
        ctx.Items.Add("-");
        ctx.Items.Add("🔒 Khoá / Mở phòng", null, (_, _) => ToggleRoom());
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
            _branches = await _service.GetAllAsync();
            _cboBranch.Items.Clear();
            _cboBranch.Items.Add("Tất cả chi nhánh");
            foreach (var b in _branches) _cboBranch.Items.Add(b);
            _cboBranch.DisplayMember = "BranchName";
            if (_cboBranch.SelectedIndex < 0) _cboBranch.SelectedIndex = 0;

            _rooms = await _service.GetAllRoomsAsync();
            RefreshGrid(_rooms);
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
        finally { Cursor = Cursors.Default; }
    }

    private async Task FilterByBranch()
    {
        try
        {
            int? branchId = _cboBranch.SelectedItem is BranchDto b ? b.Id : null;
            _rooms = await _service.GetAllRoomsAsync(branchId);
            RefreshGrid(_rooms);
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
    }

    private void RefreshGrid(List<RoomDto> list)
    {
        _grid.Rows.Clear();
        foreach (var r in list)
        {
            _grid.Rows.Add(r.Id, r.RoomName, r.BranchName, RoomTypeLabel(r.RoomType), r.Capacity, r.IsActive ? "Sẵn sàng" : "Tạm đóng");
            _grid.Rows[^1].Tag = r;
            if (!r.IsActive) _grid.Rows[^1].DefaultCellStyle.ForeColor = AppTheme.TextMuted;
        }
        _status.Text = $"Hiển thị {list.Count} phòng.  F5: Làm mới  |  F2: Sửa  |  Ctrl+N: Thêm";
    }

    private static string RoomTypeLabel(RoomType t) => t switch
    {
        RoomType.Standard   => "Phòng thường",
        RoomType.Projector  => "Phòng chiếu",
        RoomType.Lab        => "Phòng Lab",
        RoomType.Auditorium => "Hội trường",
        RoomType.Online     => "Phòng Online",
        _ => t.ToString()
    };

    private void EditSelected()
    {
        if (_grid.CurrentRow?.Tag is RoomDto r) ShowForm(r);
    }

    private void ShowForm(RoomDto? room)
    {
        using var form = new RoomFormDialog(room, _service, _branches);
        if (form.ShowDialog(this) == DialogResult.OK)
            _ = FilterByBranch();
    }

    private void ToggleRoom()
    {
        if (_grid.CurrentRow?.Tag is not RoomDto r) return;
        var action = r.IsActive ? "tạm đóng" : "mở lại";
        if (!PasswordConfirmDialog.Confirm(this, $"Nhập mật khẩu để {action} phòng '{r.RoomName}':")) return;
        _ = DoToggle(r);
    }

    private async Task DoToggle(RoomDto r)
    {
        var ok = await _service.UpdateRoomAsync(r.Id, new UpdateRoomDto
        {
            RoomName = r.RoomName,
            RoomType = r.RoomType,
            Capacity = r.Capacity,
            IsActive = !r.IsActive
        });
        if (ok) await FilterByBranch();
        else MessageBox.Show("Thao tác thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static void BorderBottom(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}

// ── Form thêm / sửa phòng ─────────────────────────────────────────────────────
internal sealed class RoomFormDialog : Form
{
    private readonly RoomDto? _existing;
    private readonly BranchService _service;
    private readonly List<BranchDto> _branches;

    private readonly ComboBox _cboBranch = new();
    private readonly TextBox _txtName = new();
    private readonly ComboBox _cboType = new();
    private readonly NumericUpDown _numCapacity = new();
    private readonly Label _error = new();

    public RoomFormDialog(RoomDto? existing, BranchService service, List<BranchDto> branches)
    {
        _existing = existing;
        _service = service;
        _branches = branches;
        Text = existing == null ? "Thêm Phòng học" : "Sửa Phòng học";
        Size = new Size(460, 310);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Build();
        if (existing != null) Fill(existing);
    }

    private void Build()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            ColumnCount = 2,
            RowCount = 6
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Branch
        _cboBranch.Items.Add("— Chọn chi nhánh —");
        foreach (var b in _branches) _cboBranch.Items.Add(b);
        _cboBranch.DisplayMember = "BranchName";
        _cboBranch.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboBranch.SelectedIndex = 0;

        // Room type
        _cboType.Items.AddRange(new object[]
        {
            RoomType.Standard, RoomType.Projector, RoomType.Lab, RoomType.Auditorium, RoomType.Online
        });
        _cboType.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboType.SelectedIndex = 0;
        _cboType.Format += (_, e) =>
        {
            if (e.ListItem is RoomType rt)
                e.Value = rt switch
                {
                    RoomType.Standard   => "Phòng thường",
                    RoomType.Projector  => "Phòng chiếu",
                    RoomType.Lab        => "Phòng Lab",
                    RoomType.Auditorium => "Hội trường",
                    RoomType.Online     => "Phòng Online",
                    _ => rt.ToString()
                };
        };

        // Capacity
        _numCapacity.Minimum = 1; _numCapacity.Maximum = 200; _numCapacity.Value = 20;

        AddRow(table, "Chi nhánh *", _cboBranch, 0);
        AddRow(table, "Tên phòng *", _txtName, 1);
        AddRow(table, "Loại phòng *", _cboType, 2);
        AddRow(table, "Sức chứa *", _numCapacity, 3);

        _error.ForeColor = AppTheme.Danger;
        _error.Font = AppTheme.FontSmall;
        _error.AutoSize = true;
        table.Controls.Add(_error, 0, 4);
        table.SetColumnSpan(_error, 2);

        var foot = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 16, 0)
        };
        var save = AppTheme.MakePrimaryBtn("Lưu", 90, 34);
        var cancel = AppTheme.MakeOutlineBtn("Hủy", 90, 34);
        save.Click += async (_, _) => await SaveAsync();
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        foot.Controls.AddRange(new Control[] { save, cancel });

        Controls.Add(table);
        Controls.Add(foot);
    }

    private static void AddRow(TableLayoutPanel t, string label, Control ctrl, int row)
    {
        t.Controls.Add(new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = AppTheme.FontBody
        }, 0, row);
        ctrl.Dock = DockStyle.Fill;
        ctrl.Font = AppTheme.FontBody;
        t.Controls.Add(ctrl, 1, row);
    }

    private void Fill(RoomDto r)
    {
        for (int i = 1; i < _cboBranch.Items.Count; i++)
        {
            if (_cboBranch.Items[i] is BranchDto b && b.Id == r.BranchId)
            { _cboBranch.SelectedIndex = i; break; }
        }
        _txtName.Text = r.RoomName;
        for (int i = 0; i < _cboType.Items.Count; i++)
        {
            if (_cboType.Items[i] is RoomType rt && rt == r.RoomType)
            { _cboType.SelectedIndex = i; break; }
        }
        _numCapacity.Value = r.Capacity;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text) || _cboBranch.SelectedIndex <= 0)
        { _error.Text = "Vui lòng chọn chi nhánh và nhập tên phòng."; return; }

        if (_cboBranch.SelectedItem is not BranchDto branch)
        { _error.Text = "Vui lòng chọn chi nhánh hợp lệ."; return; }

        var roomType = _cboType.SelectedItem is RoomType rt ? rt : RoomType.Standard;

        bool ok;
        if (_existing == null)
        {
            ok = await _service.CreateRoomAsync(new CreateRoomDto
            {
                BranchId = branch.Id,
                RoomName = _txtName.Text.Trim(),
                RoomType = roomType,
                Capacity = (int)_numCapacity.Value
            });
        }
        else
        {
            ok = await _service.UpdateRoomAsync(_existing.Id, new UpdateRoomDto
            {
                RoomName = _txtName.Text.Trim(),
                RoomType = roomType,
                Capacity = (int)_numCapacity.Value,
                IsActive = _existing.IsActive
            });
        }

        if (ok) DialogResult = DialogResult.OK;
        else _error.Text = "Lưu thất bại. Kiểm tra lại dữ liệu.";
    }
}
