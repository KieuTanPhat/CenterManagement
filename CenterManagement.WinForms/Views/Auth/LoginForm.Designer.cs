#nullable disable

namespace CenterManagement.WinForms.Views.Auth;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlLeft;
    private Panel pnlRight;
    private PictureBox picLogo;
    private TextBox txtUsername;
    private TextBox txtPassword;
    private Button btnLogin;
    private Label lblError;
    private Label lblFooter;
    private Label lblWelcome;
    private Label lblWelcomeSub;
    private Label lblUsernameHint;
    private Label lblPasswordHint;
    private Panel pnlUserWrap;
    private Panel pnlPassWrap;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        pnlLeft = new Panel();
        pnlRight = new Panel();
        picLogo = new PictureBox();
        lblWelcome = new Label();
        lblWelcomeSub = new Label();
        lblUsernameHint = new Label();
        lblPasswordHint = new Label();
        pnlUserWrap = new Panel();
        pnlPassWrap = new Panel();
        txtUsername = new TextBox();
        txtPassword = new TextBox();
        btnLogin = new Button();
        lblError = new Label();
        lblFooter = new Label();
        ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
        SuspendLayout();

        ClientSize = new Size(900, 540);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EN-VN Center - Dang nhap";
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);

        pnlLeft.Location = new Point(0, 0);
        pnlLeft.Size = new Size(320, 540);
        pnlLeft.BackColor = Color.FromArgb(200, 16, 46);
        pnlLeft.Paint += PnlLeft_Paint;

        picLogo.Location = new Point(75, 44);
        picLogo.Size = new Size(170, 170);
        picLogo.SizeMode = PictureBoxSizeMode.Zoom;
        picLogo.BackColor = Color.Transparent;
        picLogo.TabStop = false;

        pnlRight.Location = new Point(320, 0);
        pnlRight.Size = new Size(580, 540);
        pnlRight.BackColor = Color.White;

        lblWelcome.Text = "Chao mung tro lai";
        lblWelcome.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        lblWelcome.ForeColor = Color.FromArgb(30, 34, 50);
        lblWelcome.Location = new Point(70, 76);
        lblWelcome.Size = new Size(440, 42);

        lblWelcomeSub.Text = "Dang nhap bang tai khoan quan ly, nhan vien hoac giao vien";
        lblWelcomeSub.Font = new Font("Segoe UI", 9.5F);
        lblWelcomeSub.ForeColor = Color.FromArgb(105, 110, 135);
        lblWelcomeSub.Location = new Point(72, 122);
        lblWelcomeSub.Size = new Size(440, 24);

        lblUsernameHint.Text = "TEN DANG NHAP";
        lblUsernameHint.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        lblUsernameHint.ForeColor = Color.FromArgb(148, 152, 170);
        lblUsernameHint.Location = new Point(72, 166);
        lblUsernameHint.Size = new Size(200, 18);

        pnlUserWrap.Location = new Point(72, 184);
        pnlUserWrap.Size = new Size(430, 44);
        pnlUserWrap.BackColor = Color.White;
        pnlUserWrap.Paint += InputWrap_Paint;

        txtUsername.PlaceholderText = "Nhap ten dang nhap";
        txtUsername.Location = new Point(0, 10);
        txtUsername.Size = new Size(430, 26);
        txtUsername.BorderStyle = BorderStyle.None;
        txtUsername.Font = new Font("Segoe UI", 11F);
        txtUsername.BackColor = Color.White;
        txtUsername.TabIndex = 0;
        txtUsername.KeyDown += txtUsername_KeyDown;

        lblPasswordHint.Text = "MAT KHAU";
        lblPasswordHint.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        lblPasswordHint.ForeColor = Color.FromArgb(148, 152, 170);
        lblPasswordHint.Location = new Point(72, 242);
        lblPasswordHint.Size = new Size(200, 18);

        pnlPassWrap.Location = new Point(72, 260);
        pnlPassWrap.Size = new Size(430, 44);
        pnlPassWrap.BackColor = Color.White;
        pnlPassWrap.Paint += InputWrap_Paint;

        txtPassword.PlaceholderText = "Nhap mat khau";
        txtPassword.Location = new Point(0, 10);
        txtPassword.Size = new Size(430, 26);
        txtPassword.BorderStyle = BorderStyle.None;
        txtPassword.Font = new Font("Segoe UI", 11F);
        txtPassword.BackColor = Color.White;
        txtPassword.PasswordChar = '*';
        txtPassword.TabIndex = 1;
        txtPassword.KeyDown += txtPassword_KeyDown;

        btnLogin.Text = "DANG NHAP";
        btnLogin.Location = new Point(72, 334);
        btnLogin.Size = new Size(430, 48);
        btnLogin.FlatStyle = FlatStyle.Flat;
        btnLogin.BackColor = Color.FromArgb(200, 16, 46);
        btnLogin.ForeColor = Color.White;
        btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnLogin.Cursor = Cursors.Hand;
        btnLogin.TabIndex = 2;
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Click += btnLogin_Click;

        lblError.Text = "";
        lblError.ForeColor = Color.FromArgb(183, 28, 28);
        lblError.Font = new Font("Segoe UI", 8.5F);
        lblError.Location = new Point(72, 390);
        lblError.Size = new Size(430, 42);
        lblError.Visible = false;

        lblFooter.Text = "EN-VN Center - Student accounts do not sign in to this app";
        lblFooter.Font = new Font("Segoe UI", 8F);
        lblFooter.ForeColor = Color.FromArgb(160, 160, 175);
        lblFooter.TextAlign = ContentAlignment.MiddleCenter;
        lblFooter.Location = new Point(0, 506);
        lblFooter.Size = new Size(580, 22);

        pnlUserWrap.Controls.Add(txtUsername);
        pnlPassWrap.Controls.Add(txtPassword);
        pnlLeft.Controls.Add(picLogo);
        pnlRight.Controls.Add(lblWelcome);
        pnlRight.Controls.Add(lblWelcomeSub);
        pnlRight.Controls.Add(lblUsernameHint);
        pnlRight.Controls.Add(pnlUserWrap);
        pnlRight.Controls.Add(lblPasswordHint);
        pnlRight.Controls.Add(pnlPassWrap);
        pnlRight.Controls.Add(btnLogin);
        pnlRight.Controls.Add(lblError);
        pnlRight.Controls.Add(lblFooter);
        Controls.Add(pnlLeft);
        Controls.Add(pnlRight);

        ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
        ResumeLayout(false);
    }
}
