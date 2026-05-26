using CenterManagement.Models.DTOs;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Notifications;

public class NotificationControl : UserControl
{
    private readonly NotificationService _service = new();
    private DataGridView _grid = new();
    private Label _status = new();
    private List<NotificationDto> _items = new();

    public NotificationControl()
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
        var head = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        head.Paint += BorderBottom;

        var title = new Label { Text = "🔔  Thông báo", Font = AppTheme.FontSubtitle, ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true };
        var btnNew = AppTheme.MakePrimaryBtn("+ Gửi thông báo", 150, 34);
        btnNew.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnNew.Click += (_, _) => ShowCreateDialog();
        head.Controls.AddRange(new Control[] { title, btnNew });
        head.Resize += (_, _) => btnNew.Location = new Point(head.Width - 170, 13);

        _grid = AppTheme.MakeGrid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Tiêu đề", FillWeight = 25 },
            new DataGridViewTextBoxColumn { HeaderText = "Nội dung", FillWeight = 35 },
            new DataGridViewTextBoxColumn { HeaderText = "Loại", FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Đến", FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Người gửi", FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Thời gian", FillWeight = 13 },
            new DataGridViewCheckBoxColumn { HeaderText = "Đã đọc", FillWeight = 8 }
        );
        _grid.DoubleClick += (_, _) => MarkSelectedRead();

        // Right-click
        var ctxMenu = new ContextMenuStrip();
        ctxMenu.Items.Add("✅ Đánh dấu đã đọc", null, (_, _) => MarkSelectedRead());
        _grid.ContextMenuStrip = ctxMenu;

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(248, 249, 252) };
        _status = new Label { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary };
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
            _items = await _service.GetAllAsync();
            RefreshGrid();
        }
        catch (Exception ex)
        {
            _status.Text = $"Lỗi tải thông báo: {ex.Message}";
        }
        finally { Cursor = Cursors.Default; }
    }

    private void RefreshGrid()
    {
        _grid.Rows.Clear();
        foreach (var n in _items)
        {
            _grid.Rows.Add(n.Title, n.Message, n.NotificationType ?? "System",
                n.TargetUserName ?? "Tất cả", n.CreatedByName, n.CreatedAt.ToString("dd/MM/yyyy HH:mm"), n.IsRead);
            _grid.Rows[^1].Tag = n;
        }
        _status.Text = $"Hiển thị {_items.Count} thông báo. Double-click để đánh dấu đã đọc.";
    }

    private async void MarkSelectedRead()
    {
        if (_grid.CurrentRow?.Tag is not NotificationDto n) return;
        if (n.IsRead) return;
        await _service.MarkReadAsync(n.Id);
        await LoadAsync();
    }

    private void ShowCreateDialog()
    {
        using var form = new SendNotificationDialog(_service);
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

internal sealed class SendNotificationDialog : Form
{
    private readonly NotificationService _service;
    private readonly TextBox _txtTitle = new();
    private readonly TextBox _txtMessage = new();
    private readonly ComboBox _cmbType = new();
    private readonly Label _error = new();

    public SendNotificationDialog(NotificationService service)
    {
        _service = service;
        Text = "Gửi thông báo";
        Size = new Size(500, 320);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Build();
    }

    private void Build()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 5 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _cmbType.Items.AddRange(new object[] { "System", "ClassAlert", "Payment", "LeaveRequest", "General" });
        _cmbType.SelectedIndex = 0;
        _cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
        _txtMessage.Multiline = true;
        _txtMessage.Height = 60;

        AddRow(table, "Tiêu đề *", _txtTitle, 0);
        AddRow(table, "Loại", _cmbType, 1);
        AddRow(table, "Nội dung *", _txtMessage, 2);
        _error.ForeColor = AppTheme.Danger;
        _error.Font = AppTheme.FontSmall;
        _error.AutoSize = true;
        table.Controls.Add(_error, 0, 3);
        table.SetColumnSpan(_error, 2);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 16, 0) };
        var save = AppTheme.MakePrimaryBtn("Gửi", 90, 34);
        var cancel = AppTheme.MakeOutlineBtn("Hủy", 90, 34);
        save.Click += async (_, _) => await SendAsync();
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.AddRange(new Control[] { save, cancel });

        Controls.Add(table);
        Controls.Add(buttons);
    }

    private static void AddRow(TableLayoutPanel t, string label, Control ctrl, int row)
    {
        t.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontBody }, 0, row);
        ctrl.Dock = DockStyle.Fill;
        ctrl.Font = AppTheme.FontBody;
        t.Controls.Add(ctrl, 1, row);
    }

    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtTitle.Text) || string.IsNullOrWhiteSpace(_txtMessage.Text))
        { _error.Text = "Tiêu đề và nội dung không được để trống."; return; }

        var ok = await _service.CreateAsync(new CreateNotificationDto
        {
            Title = _txtTitle.Text.Trim(),
            Message = _txtMessage.Text.Trim(),
            NotificationType = _cmbType.SelectedItem?.ToString(),
            TargetUserId = null    // Broadcast
        });
        if (ok) DialogResult = DialogResult.OK;
        else _error.Text = "Gửi thất bại.";
    }
}
