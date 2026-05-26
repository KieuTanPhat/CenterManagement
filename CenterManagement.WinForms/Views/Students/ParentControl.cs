using CenterManagement.Models.DTOs;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Students;

public class ParentControl : UserControl
{
    private readonly StudentService _service = new();
    private DataGridView _grid = new();
    private TextBox _search = new();
    private Label _status = new();
    private List<StudentDto> _students = new();

    public ParentControl()
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
        head.Paint += (s, e) => { using var p = new Pen(AppTheme.Border); e.Graphics.DrawLine(p, 0, 59, ((Control)s!).Width, 59); };
        head.Controls.Add(new Label { Text = "👨‍👩‍👧  Quản lý Phụ huynh", Font = AppTheme.FontSubtitle, ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true });

        var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(250, 250, 252) };
        bar.Paint += (s, e) => { using var p = new Pen(AppTheme.Border); e.Graphics.DrawLine(p, 0, 51, ((Control)s!).Width, 51); };
        _search = new TextBox { PlaceholderText = "Tìm theo tên học sinh hoặc tên phụ huynh...", Location = new Point(16, 12), Size = new Size(300, 28), Font = AppTheme.FontBody };
        var btn = AppTheme.MakePrimaryBtn("Tìm", 80, 28);
        btn.Location = new Point(328, 12);
        btn.Click += async (_, _) => await SearchAsync();
        _search.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) await SearchAsync(); };
        bar.Controls.AddRange(new Control[] { _search, btn });

        _grid = AppTheme.MakeGrid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Mã HS", FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Tên học sinh", FillWeight = 22 },
            new DataGridViewTextBoxColumn { HeaderText = "Tên phụ huynh", FillWeight = 22 },
            new DataGridViewTextBoxColumn { HeaderText = "SĐT phụ huynh", FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Email HS", FillWeight = 18 },
            new DataGridViewTextBoxColumn { HeaderText = "SĐT HS", FillWeight = 13 }
        );

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(248, 249, 252) };
        _status = new Label { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary };
        foot.Controls.Add(_status);

        card.Controls.Add(_grid);
        card.Controls.Add(foot);
        card.Controls.Add(bar);
        card.Controls.Add(head);
        Controls.Add(card);
    }

    private async Task LoadAsync()
    {
        _students = await _service.GetAllAsync();
        RefreshGrid(_students);
    }

    private async Task SearchAsync()
    {
        _students = await _service.GetAllAsync(_search.Text.Trim());
        RefreshGrid(_students);
    }

    private void RefreshGrid(List<StudentDto> rows)
    {
        _grid.Rows.Clear();
        foreach (var s in rows)
        {
            if (string.IsNullOrEmpty(s.ParentName) && string.IsNullOrEmpty(s.ParentPhone)) continue;
            _grid.Rows.Add(s.StudentCode, s.FullName, s.ParentName ?? "—", s.ParentPhone ?? "—", s.Email ?? "—", s.Phone ?? "—");
        }
        _status.Text = $"Hiển thị {_grid.Rows.Count} học sinh có thông tin phụ huynh.";
    }
}
