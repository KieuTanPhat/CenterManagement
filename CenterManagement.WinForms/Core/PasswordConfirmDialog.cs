using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Core;

/// <summary>
/// Dialog yêu cầu nhập lại mật khẩu trước khi thực hiện thao tác rủi ro cao.
/// </summary>
public sealed class zPasswordConfirmDialog : Form
{
    private readonly TextBox _txtPassword = new();
    private readonly Label _lblError = new();
    private readonly string _message;

    public PasswordConfirmDialog(string message = "Nhập lại mật khẩu để xác nhận thao tác:")
    {
        _message = message;
        Text = "Xác nhận bảo mật";
        Size = new Size(400, 220);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BuildUI();
    }

    private void BuildUI()
    {
        var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

        var icon = new Label
        {
            Text = "🔐", Font = new Font("Segoe UI Emoji", 24F),
            Location = new Point(20, 16), AutoSize = true
        };
        var lblMsg = new Label
        {
            Text = _message,
            Font = AppTheme.FontBody,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(70, 20),
            Size = new Size(290, 40),
            TextAlign = ContentAlignment.MiddleLeft
        };
        var lblPwd = new Label
        {
            Text = "Mật khẩu:",
            Font = AppTheme.FontBodyBold,
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(20, 75),
            AutoSize = true
        };
        _txtPassword.Location = new Point(20, 96);
        _txtPassword.Size = new Size(340, 28);
        _txtPassword.Font = AppTheme.FontBody;
        _txtPassword.UseSystemPasswordChar = true;
        _txtPassword.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) TryConfirm(); if (e.KeyCode == Keys.Escape) DialogResult = DialogResult.Cancel; };

        _lblError.Font = AppTheme.FontSmall;
        _lblError.ForeColor = AppTheme.Danger;
        _lblError.Location = new Point(20, 130);
        _lblError.Size = new Size(340, 18);

        var btnOk = AppTheme.MakePrimaryBtn("Xác nhận", 110, 34);
        btnOk.Location = new Point(130, 148);
        btnOk.Click += (_, _) => TryConfirm();

        var btnCancel = AppTheme.MakeOutlineBtn("Hủy", 90, 34);
        btnCancel.Location = new Point(250, 148);
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[] { icon, lblMsg, lblPwd, _txtPassword, _lblError, btnOk, btnCancel });
        ActiveControl = _txtPassword;
    }

    private void TryConfirm()
    {
        if (string.IsNullOrWhiteSpace(_txtPassword.Text))
        {
            _lblError.Text = "Vui lòng nhập mật khẩu.";
            return;
        }
        _ = VerifyAsync(_txtPassword.Text);
    }

    private async Task VerifyAsync(string password)
    {
        try
        {
            var authService = new AuthService();
            var result = await authService.LoginAsync(AppSession.Current?.Username ?? "", password);
            if (result != null)
                DialogResult = DialogResult.OK;
            else
                _lblError.Text = "Mật khẩu không đúng. Thử lại.";
        }
        catch
        {
            _lblError.Text = "Mật khẩu không đúng. Thử lại.";
        }
    }

    /// <summary>
    /// Hiển thị dialog xác nhận mật khẩu và trả về true nếu xác nhận thành công.
    /// </summary>
    public static bool Confirm(IWin32Window? owner = null, string message = "Xác nhận bằng mật khẩu để tiếp tục:")
    {
        using var dlg = new PasswordConfirmDialog(message);
        return dlg.ShowDialog(owner) == DialogResult.OK;
    }
}
