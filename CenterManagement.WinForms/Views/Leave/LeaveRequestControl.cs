using CenterManagement.WinForms.Core;

namespace CenterManagement.WinForms.Views.Leave;

/// <summary>Đơn xin nghỉ – dành cho giáo viên (role 4).</summary>
public class LeaveRequestControl : UserControl
{
    private readonly int _roleId;

    public LeaveRequestControl(int roleId = 0)
    {
        _roleId   = roleId;
        BackColor = AppTheme.ContentBg;
        Dock      = DockStyle.Fill;
        BuildUI();
    }

    private void BuildUI()
    {
        // ── Phần bên trái: form gửi đơn ────────────────────────────────────
        var pnlLeft = new Panel { Dock = DockStyle.Left, Width = 420, BackColor = Color.White };

        var pnlFormHead = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        pnlFormHead.Paint += B;
        pnlFormHead.Controls.Add(new Label
        {
            Text = "Gửi đơn xin nghỉ", Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true
        });

        var pnlForm = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = Color.White };

        int y = 0;
        void Row(string label, Control ctrl, int h = 36)
        {
            var lbl = new Label { Text = label, Font = AppTheme.FontBodyBold, ForeColor = AppTheme.TextSecondary, Location = new Point(0, y), Size = new Size(380, 22) };
            ctrl.Location = new Point(0, y + 24);
            ctrl.Size     = new Size(380, h);
            pnlForm.Controls.AddRange(new Control[] { lbl, ctrl });
            y += h + 40;
        }

        var cmbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        cmbType.Items.AddRange(new object[] { "Nghỉ phép thường niên", "Nghỉ ốm", "Nghỉ việc cá nhân", "Nghỉ không lương" });
        cmbType.SelectedIndex = 0;

        var dtpFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = AppTheme.FontBody };
        var dtpTo   = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = AppTheme.FontBody };

        var txtReason = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Font = AppTheme.FontBody };

        var cmbSubstitute = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        cmbSubstitute.Items.AddRange(new object[] { "— Chọn giáo viên thay thế —", "Trần Minh Tuấn", "Phạm Quốc Bảo", "Lê Thị Hương" });
        cmbSubstitute.SelectedIndex = 0;

        Row("Loại nghỉ phép",              cmbType);
        Row("Từ ngày",                      dtpFrom);
        Row("Đến ngày",                     dtpTo);
        Row("Lý do (mô tả chi tiết)",       txtReason, 80);
        Row("Giáo viên dạy thay (nếu có)", cmbSubstitute);

        var btnSend = AppTheme.MakePrimaryBtn("Gửi đơn", 140, 42);
        btnSend.Location = new Point(0, y + 8);
        btnSend.Size     = new Size(380, 42);
        pnlForm.Controls.Add(btnSend);

        pnlLeft.Controls.Add(pnlForm);
        pnlLeft.Controls.Add(pnlFormHead);

        // Đường phân cách
        var sep = new Panel { Dock = DockStyle.Left, Width = 1, BackColor = AppTheme.Border };

        // ── Phần bên phải: danh sách đơn đã gửi ────────────────────────────
        var pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        var pnlListHead = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        pnlListHead.Paint += B;
        pnlListHead.Controls.Add(new Label
        {
            Text = "Lịch sử đơn xin nghỉ", Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true
        });

        var dgv = AppTheme.MakeGrid();
        dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Ngày gửi",     FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Loại nghỉ",    FillWeight = 22 },
            new DataGridViewTextBoxColumn { HeaderText = "Từ ngày",      FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Đến ngày",     FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái",   FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Ghi chú",      FillWeight = 18 }
        );
        dgv.Rows.Add("20/05/2024", "Nghỉ ốm",            "22/05/2024", "22/05/2024", "Đã duyệt",   "");
        dgv.Rows.Add("10/04/2024", "Nghỉ việc cá nhân",  "15/04/2024", "16/04/2024", "Đã duyệt",   "Có GV thay");
        dgv.Rows.Add("01/06/2024", "Nghỉ phép thường niên","05/06/2024","07/06/2024", "Chờ duyệt",  "");

        dgv.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 4) return;
            var val = dgv.Rows[e.RowIndex].Cells[4].Value?.ToString();
            if (e.CellStyle == null) return;
            e.CellStyle.ForeColor = val == "Đã duyệt" ? AppTheme.Success
                : val == "Từ chối" ? AppTheme.Danger : AppTheme.Warning;
            e.CellStyle.Font = AppTheme.FontBodyBold;
        };

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        pnlGrid.Controls.Add(dgv);

        pnlRight.Controls.Add(pnlGrid);
        pnlRight.Controls.Add(pnlListHead);

        Controls.Add(pnlRight);
        Controls.Add(sep);
        Controls.Add(pnlLeft);
    }

    private static void B(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}
