using CenterManagement.WinForms.Core;

namespace CenterManagement.WinForms.Views.Shared;

/// <summary>
/// Base cho các UserControl dạng CRUD danh sách.
/// Subclass gọi Build() trong constructor.
/// </summary>
public abstract class CrudControlBase : UserControl
{
    protected DataGridView Dgv { get; private set; } = null!;

    protected void Build(
        string title,
        string searchHint,
        string[] columns,
        int[]   columnWeights,
        string[]? filterItems = null,
        bool hasActionCol     = true)
    {
        BackColor = AppTheme.ContentBg;
        Dock      = DockStyle.Fill;

        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        // ── Header ────────────────────────────────────────────────────────────
        var pnlHead = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
        pnlHead.Paint += BottomBorder;

        var lblTitle = new Label
        {
            Text = title, Font = AppTheme.FontSubtitle,
            ForeColor = AppTheme.TextPrimary, Location = new Point(20, 16), AutoSize = true
        };
        var btnAdd = AppTheme.MakePrimaryBtn("+ Thêm mới", 120, 36);
        btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        pnlHead.Controls.AddRange(new Control[] { lblTitle, btnAdd });
        pnlHead.Resize += (s, e) => btnAdd.Location = new Point(pnlHead.Width - 140, 12);

        // ── Toolbar ───────────────────────────────────────────────────────────
        var pnlBar = new Panel
        {
            Dock = DockStyle.Top, Height = 52,
            BackColor = Color.FromArgb(250, 250, 252)
        };
        pnlBar.Paint += BottomBorder;

        var txt = new TextBox
        {
            PlaceholderText = searchHint,
            Location = new Point(16, 12), Size = new Size(280, 28),
            Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle
        };
        pnlBar.Controls.Add(txt);
        int nextX = 308;

        if (filterItems != null)
        {
            var cmb = new ComboBox
            {
                Location = new Point(nextX, 12), Size = new Size(160, 28),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody
            };
            cmb.Items.AddRange(filterItems.Cast<object>().ToArray());
            cmb.SelectedIndex = 0;
            pnlBar.Controls.Add(cmb);
            nextX += 172;
        }

        var btnSearch = AppTheme.MakePrimaryBtn("Tìm kiếm", 100, 28);
        btnSearch.Location = new Point(nextX, 12);
        pnlBar.Controls.Add(btnSearch);

        // ── Grid ──────────────────────────────────────────────────────────────
        Dgv = AppTheme.MakeGrid();
        for (int i = 0; i < columns.Length; i++)
        {
            Dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = columns[i],
                FillWeight = i < columnWeights.Length ? columnWeights[i] : 15
            });
        }
        if (hasActionCol)
            Dgv.Columns.Add(new DataGridViewButtonColumn
            {
                HeaderText = "Thao tác", Text = "Sửa",
                UseColumnTextForButtonValue = true, FillWeight = 10
            });

        AddSampleRows();

        // ── Pagination ────────────────────────────────────────────────────────
        var pnlPage = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Color.White };
        pnlPage.Paint += TopBorder;

        var lblInfo = new Label
        {
            Text = "Đang hiển thị trang 1", Font = AppTheme.FontBody,
            ForeColor = AppTheme.TextSecondary,
            Location = new Point(16, 0), Size = new Size(220, 44),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var btnPrev = AppTheme.MakeOutlineBtn("◀  Trước", 90, 30);
        btnPrev.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        var btnNext = AppTheme.MakeOutlineBtn("Sau  ▶", 90, 30);
        btnNext.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        pnlPage.Controls.AddRange(new Control[] { lblInfo, btnPrev, btnNext });
        pnlPage.Resize += (s, e) =>
        {
            btnNext.Location = new Point(pnlPage.Width - 100, 7);
            btnPrev.Location = new Point(pnlPage.Width - 198, 7);
        };

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        pnlGrid.Controls.Add(Dgv);

        card.Controls.Add(pnlGrid);
        card.Controls.Add(pnlPage);
        card.Controls.Add(pnlBar);
        card.Controls.Add(pnlHead);
        Controls.Add(card);
    }

    // Override để thêm dữ liệu mẫu
    protected virtual void AddSampleRows() { }

    protected static void BottomBorder(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, c.Height - 1, c.Width, c.Height - 1);
    }

    protected static void TopBorder(object? s, PaintEventArgs e)
    {
        if (s is not Control c) return;
        using var p = new Pen(AppTheme.Border, 1);
        e.Graphics.DrawLine(p, 0, 0, c.Width, 0);
    }
}
