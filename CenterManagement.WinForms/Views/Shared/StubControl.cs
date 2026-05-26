using CenterManagement.WinForms.Core;

namespace CenterManagement.WinForms.Views.Shared;

/// <summary>
/// Control placeholder cho các tính năng đang phát triển.
/// Hiển thị tiêu đề và mô tả tính năng.
/// </summary>
public class StubControl : UserControl
{
    public StubControl(string title, string description = "", string icon = "🚧")
    {
        BackColor = AppTheme.ContentBg;
        Dock = DockStyle.Fill;

        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var pnlHead = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        pnlHead.Paint += (s, e) =>
        {
            using var p = new Pen(AppTheme.Border, 1);
            e.Graphics.DrawLine(p, 0, 59, ((Control)s!).Width, 59);
        };
        pnlHead.Controls.Add(new Label
        {
            Text = $"{icon}  {title}",
            Font = AppTheme.FontSubtitle, ForeColor = AppTheme.TextPrimary,
            Location = new Point(20, 16), AutoSize = true
        });

        var pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var lbl = new Label
        {
            Text = string.IsNullOrEmpty(description) ? "Tính năng đang được phát triển." : description,
            Font = AppTheme.FontBody,
            ForeColor = AppTheme.TextSecondary,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };
        pnlBody.Controls.Add(lbl);

        card.Controls.Add(pnlBody);
        card.Controls.Add(pnlHead);
        Controls.Add(card);
    }
}
