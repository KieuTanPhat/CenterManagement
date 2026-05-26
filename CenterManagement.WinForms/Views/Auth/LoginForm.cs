using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Presenters;
using CenterManagement.WinForms.Services;
using CenterManagement.WinForms.Views.Main;

namespace CenterManagement.WinForms.Views.Auth;

public partial class LoginForm : Form, ILoginView
{
    private readonly LoginPresenter _presenter;

    public string Username => txtUsername.Text.Trim();
    public string Password => txtPassword.Text;

    public bool IsLoading
    {
        set
        {
            btnLogin.Enabled = !value;
            btnLogin.Text = value ? "DANG XU LY..." : "DANG NHAP";
            Cursor = value ? Cursors.WaitCursor : Cursors.Default;
        }
    }

    public string ErrorMessage
    {
        set
        {
            lblError.Text = value;
            lblError.Visible = !string.IsNullOrWhiteSpace(value);
        }
    }

    public event EventHandler? LoginClicked;

    public LoginForm()
    {
        InitializeComponent();
        LoadLogo();
        _presenter = new LoginPresenter(this, new AuthService());
        _presenter.LoginSuccess += OnLoginSuccess;
    }

    public void ShowMessage(string message)
    {
        ErrorMessage = "";
    }

    public void ShowError(string error)
    {
        ErrorMessage = error;
    }

    private void LoadLogo()
    {
        try
        {
            var path = Path.Combine(Application.StartupPath, "Resources", "vn_en_logo.png");
            if (!File.Exists(path)) return;

            using var source = new Bitmap(path);
            var dest = new Bitmap(picLogo.Width, picLogo.Height);
            using var g = Graphics.FromImage(dest);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var clip = new System.Drawing.Drawing2D.GraphicsPath();
            clip.AddEllipse(0, 0, dest.Width, dest.Height);
            g.SetClip(clip);
            g.DrawImage(source, 0, 0, dest.Width, dest.Height);
            picLogo.Image = dest;
        }
        catch
        {
            // Logo is decorative; login should remain usable if the asset is missing.
        }
    }

    private void btnLogin_Click(object? sender, EventArgs e)
    {
        LoginClicked?.Invoke(this, e);
    }

    private void txtUsername_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            txtPassword.Focus();
            e.SuppressKeyPress = true;
        }
    }

    private void txtPassword_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            LoginClicked?.Invoke(this, e);
            e.SuppressKeyPress = true;
        }
    }

    private void OnLoginSuccess(object? sender, EventArgs e)
    {
        Hide();

        var main = new MainForm();
        main.LoggedOut += (_, _) =>
        {
            AppSession.Current?.Clear();
            txtPassword.Clear();
            ErrorMessage = "";
            IsLoading = false;
            Show();
            Activate();
            txtUsername.Focus();
        };
        main.FormClosed += (_, _) =>
        {
            if (!Visible)
            {
                Close();
            }
        };
        main.Show();
    }

    private void InputWrap_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Control control) return;

        using var pen = new Pen(Color.FromArgb(210, 214, 225), 1);
        e.Graphics.DrawLine(pen, 0, control.Height - 1, control.Width, control.Height - 1);
    }

    private void PnlLeft_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Control control) return;

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using var bg = new System.Drawing.Drawing2D.LinearGradientBrush(
            new Rectangle(0, 0, control.Width, control.Height),
            Color.FromArgb(225, 42, 70),
            Color.FromArgb(118, 8, 26),
            System.Drawing.Drawing2D.LinearGradientMode.Vertical);
        g.FillRectangle(bg, 0, 0, control.Width, control.Height);

        using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var white = new SolidBrush(Color.White);
        using var soft = new SolidBrush(Color.FromArgb(255, 220, 225));
        using var muted = new SolidBrush(Color.FromArgb(235, 178, 186));
        using var line = new Pen(Color.FromArgb(75, 255, 255, 255), 1);
        using var brandFont = new Font("Segoe UI", 26F, FontStyle.Bold);
        using var subFont = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        using var tagFont = new Font("Segoe UI", 8.5F);

        g.DrawLine(line, 42, 234, control.Width - 42, 234);
        g.DrawString("EN-VN", brandFont, white, new RectangleF(0, 240, control.Width, 48), center);
        g.DrawString("CENTER MANAGEMENT", subFont, soft, new RectangleF(0, 292, control.Width, 24), center);
        g.DrawLine(line, 42, 326, control.Width - 42, 326);
        g.DrawString("TP.HCM - Da Nang - Ha Noi", tagFont, muted, new RectangleF(0, 336, control.Width, 24), center);
    }
}
