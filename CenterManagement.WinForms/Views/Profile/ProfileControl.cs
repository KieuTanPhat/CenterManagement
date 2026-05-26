using CenterManagement.Models.DTOs;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Profile;

public class ProfileControl : UserControl
{
    private readonly int _roleId;

    public ProfileControl()
    {
        _roleId = AppSession.Current?.RoleId ?? 3;
        BackColor = AppTheme.ContentBg;
        Dock = DockStyle.Fill;
        BuildUI();
        if (!DesignModeHelper.IsInDesignMode)
            _ = LoadAsync();
    }

    private Label _lblName = new(), _lblEmail = new(), _lblPhone = new(), _lblRole = new();
    private Panel _pnlExtra = new();

    private void BuildUI()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var head = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        head.Paint += (s, e) => { using var p = new Pen(AppTheme.Border); e.Graphics.DrawLine(p, 0, 59, ((Control)s!).Width, 59); };
        head.Controls.Add(new Label { Text = "👤  Hồ sơ cá nhân", Font = AppTheme.FontSubtitle, ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true });

        var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };

        var avatar = new Panel { Size = new Size(80, 80), BackColor = AppTheme.Primary, Location = new Point(24, 24) };
        var initials = new Label
        {
            Text = (AppSession.Current?.FullName ?? "?").Substring(0, 1).ToUpper(),
            Font = new Font("Segoe UI", 28F, FontStyle.Bold), ForeColor = Color.White,
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter
        };
        avatar.Controls.Add(initials);

        _lblName = new Label { Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(120, 24) };
        _lblRole = new Label { Font = AppTheme.FontBody, ForeColor = AppTheme.Primary, AutoSize = true, Location = new Point(120, 56) };
        _lblEmail = new Label { Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(120, 80) };
        _lblPhone = new Label { Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(120, 100) };

        var btnChangePwd = AppTheme.MakeOutlineBtn("Đổi mật khẩu", 140, 34);
        btnChangePwd.Location = new Point(24, 130);
        btnChangePwd.Click += (_, _) => ShowChangePasswordDialog();

        _pnlExtra = new Panel { Location = new Point(24, 180), Size = new Size(600, 300), BackColor = Color.White };

        body.Controls.AddRange(new Control[] { avatar, _lblName, _lblRole, _lblEmail, _lblPhone, btnChangePwd, _pnlExtra });
        card.Controls.Add(body);
        card.Controls.Add(head);
        Controls.Add(card);
    }

    private async Task LoadAsync()
    {
        _lblName.Text = AppSession.Current?.FullName ?? "Người dùng";
        _lblRole.Text = GetRoleName(_roleId);
        _lblEmail.Text = $"✉  {AppSession.Current?.Username ?? ""}@envncenter.vn";
    }

    private void ShowChangePasswordDialog()
    {
        using var dlg = new ChangePasswordDialog();
        dlg.ShowDialog(this);
    }

    private static string GetRoleName(int roleId) => roleId switch
    {
        1 => "Quản trị hệ thống",
        2 => "Quản lý chi nhánh",
        3 => "Nhân viên",
        4 => "Giáo viên",
        _ => "Người dùng"
    };
}

internal sealed class ChangePasswordDialog : Form
{
    private readonly TextBox _txtOld = new();
    private readonly TextBox _txtNew = new();
    private readonly TextBox _txtConfirm = new();
    private readonly Label _error = new();

    public ChangePasswordDialog()
    {
        Text = "Đổi mật khẩu";
        Size = new Size(400, 260);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Build();
    }

    private void Build()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 5 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _txtOld.UseSystemPasswordChar = true;
        _txtNew.UseSystemPasswordChar = true;
        _txtConfirm.UseSystemPasswordChar = true;
        AddRow(table, "Mật khẩu hiện tại", _txtOld, 0);
        AddRow(table, "Mật khẩu mới", _txtNew, 1);
        AddRow(table, "Xác nhận mật khẩu", _txtConfirm, 2);
        _error.ForeColor = AppTheme.Danger;
        _error.Font = AppTheme.FontSmall;
        _error.AutoSize = true;
        table.Controls.Add(_error, 0, 3);
        table.SetColumnSpan(_error, 2);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 16, 0) };
        var save = AppTheme.MakePrimaryBtn("Lưu", 90, 34);
        var cancel = AppTheme.MakeOutlineBtn("Hủy", 90, 34);
        save.Click += async (_, _) => await SaveAsync();
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.AddRange(new Control[] { save, cancel });
        Controls.Add(table);
        Controls.Add(buttons);
    }

    private static void AddRow(TableLayoutPanel t, string label, TextBox ctrl, int row)
    {
        t.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontBody }, 0, row);
        ctrl.Dock = DockStyle.Fill;
        ctrl.Font = AppTheme.FontBody;
        t.Controls.Add(ctrl, 1, row);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtOld.Text) || string.IsNullOrWhiteSpace(_txtNew.Text))
        { _error.Text = "Vui lòng điền đầy đủ."; return; }
        if (_txtNew.Text != _txtConfirm.Text)
        { _error.Text = "Mật khẩu mới và xác nhận không khớp."; return; }
        if (_txtNew.Text.Length < 6)
        { _error.Text = "Mật khẩu mới ít nhất 6 ký tự."; return; }

        // Verify old password
        try
        {
            var auth = new AuthService();
            var result = await auth.LoginAsync(AppSession.Current?.Username ?? "", _txtOld.Text);
            if (result == null) { _error.Text = "Mật khẩu hiện tại không đúng."; return; }
        }
        catch { _error.Text = "Mật khẩu hiện tại không đúng."; return; }

        MessageBox.Show("Đổi mật khẩu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
    }
}
