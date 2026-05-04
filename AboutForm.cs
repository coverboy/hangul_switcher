using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace HangulSwitcher;

internal sealed class AboutForm : Form
{
    private const string IconFont = "Segoe MDL2 Assets";
    private const string TextFont = "Segoe UI";
    private const float  BaseFontSize = 11f;
    private static readonly Color LinkColor = Color.FromArgb(50, 100, 200);

    public AboutForm()
    {
        Text = $"{AppInfo.AppName} 정보";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = Color.White;
        ForeColor = Color.Black;
        Padding = new Padding(24);
        Font = new Font(TextFont, BaseFontSize);
        ClientSize = new Size(940, 420);

        BuildLayout();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // 폼 안 어떤 컨트롤이 포커스를 갖고 있어도 ESC/Enter 가로채서 닫음.
        if (keyData == Keys.Escape || keyData == Keys.Enter)
        {
            Close();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));

        root.Controls.Add(BuildLeftPanel(), 0, 0);
        root.Controls.Add(BuildRightPanel(), 1, 0);

        Controls.Add(root);
    }

    private static Control BuildLeftPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(0, 0, 20, 0)
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        panel.Controls.Add(new AboutLogoPanel { Dock = DockStyle.Fill }, 0, 0);

        panel.Controls.Add(new Label
        {
            Text = $"{AppInfo.AppName} 정보",
            Font = new Font(TextFont, 13f, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 16, 0, 6)
        }, 0, 1);

        panel.Controls.Add(new Label
        {
            Text = "Shift + Space로 한/영 전환\n시스템 트레이 유틸리티.",
            Font = new Font(TextFont, BaseFontSize),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Anchor = AnchorStyles.None
        }, 0, 2);

        return panel;
    }

    private static Control BuildRightPanel()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var github = BuildCard(
            iconGlyph: "\uE943",
            label: "GITHUB",
            valueText: "github.com/coverboy/hangul_switcher",
            href: AppInfo.ProjectUrl,
            extraIconGlyph: "\uE8A7");
        github.Margin = new Padding(0, 0, 0, 6);
        grid.Controls.Add(github, 0, 0);
        grid.SetColumnSpan(github, 2);

        var contact = BuildCard(
            iconGlyph: "\uE715",
            label: "CONTACT",
            valueText: AppInfo.ContactEmail,
            href: $"mailto:{AppInfo.ContactEmail}",
            extraIconGlyph: null);
        contact.Margin = new Padding(0, 6, 6, 0);
        grid.Controls.Add(contact, 0, 1);

        var license = BuildCard(
            iconGlyph: "\uE72E",
            label: "LICENSE",
            valueText: "라이선스 완전 Free.",
            href: null,
            extraIconGlyph: null);
        license.Margin = new Padding(6, 6, 0, 0);
        grid.Controls.Add(license, 1, 1);

        grid.Controls.Add(new Label
        {
            Text = "\"마음대로 가져다 쓰세요.\"",
            Font = new Font(TextFont, BaseFontSize, FontStyle.Italic),
            AutoSize = true,
            Anchor = AnchorStyles.Right | AnchorStyles.Top,
            Margin = new Padding(0, 10, 4, 0)
        }, 1, 2);

        return grid;
    }

    private static Control BuildCard(
        string iconGlyph,
        string label,
        string valueText,
        string? href,
        string? extraIconGlyph)
    {
        var card = new RoundedPanel
        {
            BackColor = Color.White,
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 14, 16, 14)
        };

        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var icon = new Label
        {
            Text = iconGlyph,
            Font = new Font(IconFont, 16f),
            AutoSize = true,
            Margin = new Padding(0, 2, 14, 0),
            Anchor = AnchorStyles.Left
        };
        inner.Controls.Add(icon, 0, 0);
        inner.SetRowSpan(icon, 2);

        inner.Controls.Add(new Label
        {
            Text = label,
            Font = new Font(TextFont, BaseFontSize, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4)
        }, 1, 0);

        Control valueControl = href != null
            ? CreateLink(valueText, href)
            : new Label
            {
                Text = valueText,
                Font = new Font(TextFont, BaseFontSize),
                AutoSize = true,
                Margin = new Padding(0)
            };
        inner.Controls.Add(valueControl, 1, 1);

        if (extraIconGlyph != null)
        {
            var extra = new Label
            {
                Text = extraIconGlyph,
                Font = new Font(IconFont, 13f),
                AutoSize = true,
                Anchor = AnchorStyles.Right
            };
            inner.Controls.Add(extra, 2, 0);
            inner.SetRowSpan(extra, 2);
        }

        card.Controls.Add(inner);
        return card;
    }

    private static LinkLabel CreateLink(string text, string href)
    {
        var link = new LinkLabel
        {
            Text = text,
            Font = new Font(TextFont, BaseFontSize),
            AutoSize = true,
            LinkColor = LinkColor,
            ActiveLinkColor = LinkColor,
            LinkBehavior = LinkBehavior.HoverUnderline,
            Margin = new Padding(0)
        };
        link.LinkClicked += (_, _) => Process.Start(new ProcessStartInfo
        {
            FileName = href,
            UseShellExecute = true
        });
        return link;
    }
}
