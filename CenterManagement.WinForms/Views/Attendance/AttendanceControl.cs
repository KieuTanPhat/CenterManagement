using CenterManagement.Models.DTOs;
using CenterManagement.Models.Enums;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Attendance;

public class AttendanceControl : UserControl
{
    private readonly int _roleId;

    public AttendanceControl(int roleId = 1)
    {
        _roleId = roleId;
        BackColor = AppTheme.ContentBg;
        Dock = DockStyle.Fill;
        BuildUI();
    }

    private void BuildUI()
    {
        if (_roleId == 4)
            Controls.Add(new TeacherAttendancePanel());
        else
            Controls.Add(new AdminAttendancePanel());
    }
}

// ── Giáo viên: chọn lớp → chọn buổi → điểm danh ─────────────────────────────
internal sealed class TeacherAttendancePanel : UserControl
{
    private readonly ClassService _classService = new();
    private readonly ScheduleService _scheduleService = new();
    private readonly AttendanceService _attendanceService = new();

    private ComboBox _cboClass = new();
    private ComboBox _cboSchedule = new();
    private DataGridView _grid = new();
    private Label _lblSummary = new();
    private Label _status = new();
    private List<ClassDto> _classes = new();
    private List<ScheduleDto> _schedules = new();

    public TeacherAttendancePanel()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        BuildUI();
        if (!DesignModeHelper.IsInDesignMode)
            _ = LoadClassesAsync();
    }

    private void BuildUI()
    {
        var head = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        head.Paint += Border;
        head.Controls.Add(new Label
        {
            Text = "✅  Điểm danh học sinh",
            Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(20, 16),
            AutoSize = true
        });

        var bar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.FromArgb(250, 250, 252) };
        bar.Paint += Border;

        var lblClass = new Label { Text = "Lớp:", Location = new Point(16, 18), AutoSize = true, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary };
        _cboClass = new ComboBox { Location = new Point(52, 14), Size = new Size(220, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        _cboClass.SelectedIndexChanged += async (_, _) => await LoadSchedulesAsync();

        var lblSched = new Label { Text = "Buổi:", Location = new Point(286, 18), AutoSize = true, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary };
        _cboSchedule = new ComboBox { Location = new Point(326, 14), Size = new Size(240, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        _cboSchedule.SelectedIndexChanged += async (_, _) => await LoadAttendanceAsync();

        bar.Controls.AddRange(new Control[] { lblClass, _cboClass, lblSched, _cboSchedule });

        _grid = AppTheme.MakeGrid();
        _grid.ReadOnly = false;
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "STT", FillWeight = 6, ReadOnly = true },
            new DataGridViewTextBoxColumn { HeaderText = "Mã HS", FillWeight = 10, ReadOnly = true },
            new DataGridViewTextBoxColumn { HeaderText = "Họ và tên", FillWeight = 28, ReadOnly = true },
            new DataGridViewComboBoxColumn
            {
                HeaderText = "Trạng thái", FillWeight = 16,
                Items = { "Có mặt", "Vắng", "Trễ", "Có phép" },
                DefaultCellStyle = { BackColor = Color.White }
            },
            new DataGridViewTextBoxColumn { HeaderText = "Ghi chú", FillWeight = 40 }
        );

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.White };
        foot.Paint += (s, e) => { if (s is Control c) { using var p = new Pen(AppTheme.Border); e.Graphics.DrawLine(p, 0, 0, c.Width, 0); } };
        _lblSummary = new Label
        {
            Location = new Point(16, 0),
            Size = new Size(400, 52),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = AppTheme.FontBody,
            ForeColor = AppTheme.TextSecondary
        };
        var btnSave = AppTheme.MakePrimaryBtn("Lưu điểm danh", 150, 36);
        btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSave.Click += async (_, _) => await SaveAsync();
        foot.Controls.AddRange(new Control[] { _lblSummary, btnSave });
        foot.Resize += (_, _) => btnSave.Location = new Point(foot.Width - 166, 8);

        var statusBar = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = Color.FromArgb(248, 249, 252) };
        _status = new Label { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary };
        statusBar.Controls.Add(_status);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        pnlGrid.Controls.Add(_grid);

        Controls.Add(pnlGrid);
        Controls.Add(foot);
        Controls.Add(statusBar);
        Controls.Add(bar);
        Controls.Add(head);
    }

    private async Task LoadClassesAsync()
    {
        try
        {
            _classes = await _classService.GetAllAsync(ClassStatus.Active);
            _cboClass.Items.Clear();
            _cboClass.Items.Add("— Chọn lớp học —");
            foreach (var c in _classes) _cboClass.Items.Add(c);
            _cboClass.DisplayMember = "ClassName";
            _cboClass.SelectedIndex = 0;
            _status.Text = $"Đã tải {_classes.Count} lớp đang hoạt động.";
        }
        catch (Exception ex) { _status.Text = $"Lỗi tải lớp: {ex.Message}"; }
    }

    private async Task LoadSchedulesAsync()
    {
        if (_cboClass.SelectedItem is not ClassDto cls) { _cboSchedule.Items.Clear(); return; }
        try
        {
            _schedules = await _scheduleService.GetAllAsync(classId: cls.Id);
            _cboSchedule.Items.Clear();
            _cboSchedule.Items.Add("— Chọn buổi học —");
            foreach (var s in _schedules)
            {
                var label = $"{s.ScheduleDate:dd/MM/yyyy} – {s.TimeSlotName} ({s.RoomName})";
                _cboSchedule.Items.Add(label);
            }
            _cboSchedule.SelectedIndex = 0;
            _grid.Rows.Clear();
            _lblSummary.Text = "";
        }
        catch (Exception ex) { _status.Text = $"Lỗi tải buổi: {ex.Message}"; }
    }

    private async Task LoadAttendanceAsync()
    {
        int idx = _cboSchedule.SelectedIndex - 1;
        if (idx < 0 || idx >= _schedules.Count) { _grid.Rows.Clear(); return; }
        var schedule = _schedules[idx];
        try
        {
            var list = await _attendanceService.GetByScheduleAsync(schedule.Id);
            _grid.Rows.Clear();
            int i = 1;
            foreach (var a in list)
            {
                _grid.Rows.Add(i++, a.StudentCode, a.StudentName, StatusLabel(a.Status), a.Note ?? "");
                _grid.Rows[^1].Tag = a.StudentId;
            }
            UpdateSummary();
            _status.Text = $"Buổi {schedule.ScheduleDate:dd/MM/yyyy} — {list.Count} học sinh.";
        }
        catch (Exception ex) { _status.Text = $"Lỗi tải điểm danh: {ex.Message}"; }
    }

    private void UpdateSummary()
    {
        int present = 0, absent = 0, late = 0, excused = 0;
        foreach (DataGridViewRow row in _grid.Rows)
        {
            var val = row.Cells[3].Value?.ToString();
            if (val == "Có mặt") present++;
            else if (val == "Vắng") absent++;
            else if (val == "Trễ") late++;
            else if (val == "Có phép") excused++;
        }
        _lblSummary.Text = $"Có mặt: {present}  |  Vắng: {absent}  |  Trễ: {late}  |  Có phép: {excused}";
    }

    private async Task SaveAsync()
    {
        int idx = _cboSchedule.SelectedIndex - 1;
        if (idx < 0 || idx >= _schedules.Count) { MessageBox.Show("Vui lòng chọn buổi học.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var schedule = _schedules[idx];

        var items = new List<AttendanceItemDto>();
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.Tag is not int studentId) continue;
            var statusStr = row.Cells[3].Value?.ToString() ?? "Có mặt";
            var note = row.Cells[4].Value?.ToString();
            items.Add(new AttendanceItemDto
            {
                StudentId = studentId,
                Status = ParseStatus(statusStr),
                Note = string.IsNullOrWhiteSpace(note) ? null : note
            });
        }

        if (items.Count == 0) { MessageBox.Show("Danh sách điểm danh trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        Cursor = Cursors.WaitCursor;
        try
        {
            var (ok, msg) = await _attendanceService.BulkMarkAsync(new BulkAttendanceDto { ScheduleId = schedule.Id, Attendances = items });
            _status.Text = ok ? $"Lưu thành công {items.Count} điểm danh." : $"Lỗi: {msg}";
            if (ok) UpdateSummary();
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
        finally { Cursor = Cursors.Default; }
    }

    private static string StatusLabel(AttendanceStatus s) => s switch
    {
        AttendanceStatus.Present => "Có mặt",
        AttendanceStatus.Absent  => "Vắng",
        AttendanceStatus.Late    => "Trễ",
        AttendanceStatus.Excused => "Có phép",
        _ => "Có mặt"
    };

    private static AttendanceStatus ParseStatus(string s) => s switch
    {
        "Vắng"    => AttendanceStatus.Absent,
        "Trễ"     => AttendanceStatus.Late,
        "Có phép" => AttendanceStatus.Excused,
        _ => AttendanceStatus.Present
    };

    private static void Border(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}

// ── Quản lý: xem lịch sử điểm danh ──────────────────────────────────────────
internal sealed class AdminAttendancePanel : UserControl
{
    private readonly ClassService _classService = new();
    private readonly ScheduleService _scheduleService = new();
    private readonly AttendanceService _attendanceService = new();

    private ComboBox _cboClass = new();
    private DataGridView _grid = new();
    private Label _status = new();
    private List<ClassDto> _classes = new();
    private List<ScheduleDto> _schedules = new();

    public AdminAttendancePanel()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        BuildUI();
        if (!DesignModeHelper.IsInDesignMode)
            _ = LoadClassesAsync();
    }

    private void BuildUI()
    {
        var head = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        head.Paint += Border;
        head.Controls.Add(new Label
        {
            Text = "✅  Lịch sử Điểm danh",
            Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary,
            Location = new Point(20, 16),
            AutoSize = true
        });

        var bar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.FromArgb(250, 250, 252) };
        bar.Paint += Border;

        var lblClass = new Label { Text = "Lớp:", Location = new Point(16, 18), AutoSize = true, Font = AppTheme.FontBody, ForeColor = AppTheme.TextSecondary };
        _cboClass = new ComboBox { Location = new Point(52, 14), Size = new Size(240, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        _cboClass.SelectedIndexChanged += async (_, _) => await LoadScheduleSummaryAsync();

        bar.Controls.AddRange(new Control[] { lblClass, _cboClass });

        _grid = AppTheme.MakeGrid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Buổi học", FillWeight = 16 },
            new DataGridViewTextBoxColumn { HeaderText = "Phòng", FillWeight = 12 },
            new DataGridViewTextBoxColumn { HeaderText = "Ca", FillWeight = 14 },
            new DataGridViewTextBoxColumn { HeaderText = "Có mặt", FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Vắng", FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Trễ", FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Có phép", FillWeight = 10 },
            new DataGridViewTextBoxColumn { HeaderText = "Tổng HS", FillWeight = 10 }
        );
        _grid.DoubleClick += async (_, _) => await ShowDetail();

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = Color.FromArgb(248, 249, 252) };
        _status = new Label { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary };
        foot.Controls.Add(_status);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        pnlGrid.Controls.Add(_grid);

        Controls.Add(pnlGrid);
        Controls.Add(foot);
        Controls.Add(bar);
        Controls.Add(head);
    }

    private async Task LoadClassesAsync()
    {
        try
        {
            _classes = await _classService.GetAllAsync();
            _cboClass.Items.Clear();
            _cboClass.Items.Add("— Chọn lớp —");
            foreach (var c in _classes) _cboClass.Items.Add(c);
            _cboClass.DisplayMember = "ClassName";
            _cboClass.SelectedIndex = 0;
            _status.Text = $"Đã tải {_classes.Count} lớp.";
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
    }

    private async Task LoadScheduleSummaryAsync()
    {
        if (_cboClass.SelectedItem is not ClassDto cls) return;
        try
        {
            _schedules = await _scheduleService.GetAllAsync(classId: cls.Id);
            _grid.Rows.Clear();
            foreach (var s in _schedules)
            {
                var att = await _attendanceService.GetByScheduleAsync(s.Id);
                int present = att.Count(a => a.Status == AttendanceStatus.Present);
                int absent  = att.Count(a => a.Status == AttendanceStatus.Absent);
                int late    = att.Count(a => a.Status == AttendanceStatus.Late);
                int excused = att.Count(a => a.Status == AttendanceStatus.Excused);
                _grid.Rows.Add(s.ScheduleDate.ToString("dd/MM/yyyy"), s.RoomName, s.TimeSlotName, present, absent, late, excused, att.Count);
                _grid.Rows[^1].Tag = s.Id;
            }
            _status.Text = $"{cls.ClassName} — {_schedules.Count} buổi. Double-click để xem chi tiết.";
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
    }

    private async Task ShowDetail()
    {
        if (_grid.CurrentRow?.Tag is not int scheduleId) return;
        try
        {
            var att = await _attendanceService.GetByScheduleAsync(scheduleId);
            using var dlg = new AttendanceDetailDialog(att);
            dlg.ShowDialog(this);
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
    }

    private static void Border(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }
}

// ── Dialog chi tiết điểm danh theo buổi ──────────────────────────────────────
internal sealed class AttendanceDetailDialog : Form
{
    public AttendanceDetailDialog(List<AttendanceDto> records)
    {
        Text = "Chi tiết điểm danh buổi học";
        Size = new Size(560, 480);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.White;

        var grid = AppTheme.MakeGrid();
        grid.Dock = DockStyle.Fill;
        grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Mã HS", FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "Họ tên", FillWeight = 35 },
            new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", FillWeight = 20 },
            new DataGridViewTextBoxColumn { HeaderText = "Ghi chú", FillWeight = 30 }
        );

        foreach (var a in records)
        {
            grid.Rows.Add(a.StudentCode, a.StudentName, StatusLabel(a.Status), a.Note ?? "");
            var clr = a.Status switch
            {
                AttendanceStatus.Absent  => Color.FromArgb(255, 230, 230),
                AttendanceStatus.Late    => Color.FromArgb(255, 248, 220),
                AttendanceStatus.Excused => Color.FromArgb(230, 245, 255),
                _ => Color.White
            };
            grid.Rows[^1].DefaultCellStyle.BackColor = clr;
        }

        var foot = new Panel { Dock = DockStyle.Bottom, Height = 44 };
        var btn = AppTheme.MakeOutlineBtn("Đóng", 90, 34);
        btn.Anchor = AnchorStyles.None;
        btn.Click += (_, _) => Close();
        var fp = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 4, 12, 0) };
        fp.Controls.Add(btn);
        foot.Controls.Add(fp);

        Controls.Add(grid);
        Controls.Add(foot);
    }

    private static string StatusLabel(AttendanceStatus s) => s switch
    {
        AttendanceStatus.Present => "Có mặt",
        AttendanceStatus.Absent  => "Vắng",
        AttendanceStatus.Late    => "Trễ",
        AttendanceStatus.Excused => "Có phép",
        _ => s.ToString()
    };
}
