using CenterManagement.WinForms.Core;

namespace CenterManagement.WinForms.Views.Users;

public class UserManageControl : UserControl
{
    private readonly int _roleId;

    public UserManageControl(int roleId = 1)
    {
        _roleId = roleId;
        BuildUI();
    }

    private void BuildUI()
    {
        BackColor = AppTheme.ContentBg;
        Dock = DockStyle.Fill;
        Padding = new Padding(20);

        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var title = new Label
        {
            Text = _roleId == 1 ? "Quan ly nguoi dung toan he thong" : "Quan ly tai khoan chi nhanh",
            Dock = DockStyle.Top,
            Height = 52,
            Padding = new Padding(18, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary
        };

        var grid = AppTheme.MakeGrid();
        grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Username", FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Ho ten", FillWeight = 22 },
            new DataGridViewTextBoxColumn { HeaderText = "Email", FillWeight = 22 },
            new DataGridViewTextBoxColumn { HeaderText = "Vai tro", FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Pham vi", FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Trang thai", FillWeight = 10 }
        );

        grid.Rows.Add("admin", "Quan tri chuoi", "admin@envn.edu.vn", "SystemAdmin", "Toan he thong", "Hoat dong");
        grid.Rows.Add("manager.hcm", "Quan ly HCM", "manager.hcm@envn.edu.vn", "BranchManager", "HCM", "Hoat dong");
        grid.Rows.Add("staff.hcm", "Nhan vien HCM", "staff.hcm@envn.edu.vn", "Staff", "HCM", "Hoat dong");
        grid.Rows.Add("teacher.ielts", "GV IELTS", "teacher.ielts@envn.edu.vn", "Teacher", "Nhieu chi nhanh", "Hoat dong");

        var note = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            Padding = new Padding(18, 0, 18, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            Text = "Quy tac: Student khong dang nhap. Teacher chi co role Teacher, co the day nhieu trung tam nhung khong dong thoi la Staff/Manager."
        };

        card.Controls.Add(grid);
        card.Controls.Add(note);
        card.Controls.Add(title);
        Controls.Add(card);
    }
}
