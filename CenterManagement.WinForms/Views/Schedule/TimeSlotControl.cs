using CenterManagement.Models.DTOs;
using CenterManagement.WinForms.Core;
using CenterManagement.WinForms.Services;

namespace CenterManagement.WinForms.Views.Schedule;

public class TimeSlotControl : UserControl
{
    private readonly ScheduleService _service = new();
    private DataGridView _grid = new();
    private Label _status = new();

    public TimeSlotControl()
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
        head.Controls.Add(new Label { Text = "⏰  Quản lý Ca học / TimeSlot", Font = AppTheme.FontSubtitle, ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true });

        _grid = AppTheme.MakeGrid();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", FillWeight = 8 },
            new DataGridViewTextBoxColumn { HeaderText = "Tên ca", FillWeight = 30 },
            new DataGridViewTextBoxColumn { HeaderText = "Giờ bắt đầu", FillWeight = 20 },
            new DataGridViewTextBoxColumn { HeaderText = "Giờ kết thúc", FillWeight = 20 },
            new DataGridViewTextBoxColumn { HeaderText = "Thời lượng", FillWeight = 22 }
        );

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
        try
        {
            var slots = await _service.GetTimeSlotsAsync();
            _grid.Rows.Clear();
            foreach (var s in slots)
            {
                var start = s.StartTime;
                var end = s.EndTime;
                var duration = $"{(end - start).TotalMinutes:0} phút";
                _grid.Rows.Add(s.Id, s.SlotName, start.ToString("HH:mm"), end.ToString("HH:mm"), duration);
            }
            _status.Text = $"Hiển thị {slots.Count} ca học.";
        }
        catch (Exception ex) { _status.Text = $"Lỗi: {ex.Message}"; }
    }
}
