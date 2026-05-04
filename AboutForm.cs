using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace HangulSwitcher;

internal sealed class AboutForm : Form
{
    private const string IconFont = "Segoe MDL2 Assets";
    private const string TextFont = "Segoe UI";
    private const float  BaseFontSize = 11f;

    private const int FormWidth   = 940;
    private const int FormHeight  = 420;

    // 좌측 영역
    private const int LeftX       = 24;
    private const int LogoSize    = 240;
    private const int LogoY       = 24;
    private const int TitleY      = 290;
    private const int SubtitleY   = 322;

    // 우측 카드 영역
    private const int RightX        = 310;
    private const int CardWidth     = 296;
    private const int CardHeight    = 130;
    private const int GithubWidth   = 606;
    private const int Row1Y         = 16;
    private const int Row2Y         = 156;
    private const int LicenseX      = 620;
    private const int QuoteY        = 305;

    // 카드 내부 좌표 (CardHeight=130 기준)
    private const int IconX     = 16;
    private const int IconY     = 70;   // 본문 vertical center 정렬
    private const int LabelX    = 64;
    private const int LabelY    = 42;   // 라벨+본문 묶음을 카드 vertical center
    private const int ValueX    = 64;
    private const int ValueY    = 73;
    private const int ExtraIconRightPad = 16;
    private const float ExtraIconFontSize = 14f;
    private const int ExtraIconSize = 19;

    private static readonly Color LinkColor       = Color.FromArgb(50, 100, 200);
    private static readonly Color GithubIconColor = Color.FromArgb(36, 41, 47);
    private static readonly Color MailIconColor   = Color.FromArgb(220, 64, 64);
    private static readonly Color ShieldIconColor = Color.FromArgb(34, 160, 85);

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
        Font = new Font(TextFont, BaseFontSize);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(FormWidth, FormHeight);

        BuildLayout();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape || keyData == Keys.Enter)
        {
            Close();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void BuildLayout()
    {
        Controls.Add(new AboutLogoPanel
        {
            Location = new Point(LeftX, LogoY),
            Size = new Size(LogoSize, LogoSize)
        });

        Controls.Add(new Label
        {
            Text = $"{AppInfo.AppName} 정보",
            Font = new Font(TextFont, 13f, FontStyle.Bold),
            Location = new Point(LeftX, TitleY),
            Size = new Size(LogoSize, 24),
            TextAlign = ContentAlignment.MiddleCenter
        });

        Controls.Add(new Label
        {
            Text = "Shift + Space로 한/영 전환\n시스템 트레이 유틸리티.",
            Font = new Font(TextFont, BaseFontSize),
            Location = new Point(LeftX, SubtitleY),
            Size = new Size(LogoSize, 50),
            TextAlign = ContentAlignment.MiddleCenter
        });

        Controls.Add(BuildCard(
            new Point(RightX, Row1Y), GithubWidth,
            iconGlyph: "\uE943",   iconColor: GithubIconColor,
            label: "GITHUB",
            valueText: "github.com/coverboy/hangul_switcher",
            href: AppInfo.ProjectUrl,
            extraIconGlyph: "\uE8A7", extraIconColor: LinkColor));

        Controls.Add(BuildCard(
            new Point(RightX, Row2Y), CardWidth,
            iconGlyph: "\uE715",   iconColor: MailIconColor,
            label: "CONTACT",
            valueText: AppInfo.ContactEmail,
            href: $"mailto:{AppInfo.ContactEmail}",
            extraIconGlyph: null,      extraIconColor: null));

        Controls.Add(BuildCard(
            new Point(LicenseX, Row2Y), CardWidth,
            iconGlyph: "\uE72E", iconColor: ShieldIconColor,
            label: "LICENSE",
            valueText: "라이선스 완전 Free.",
            href: null,
            extraIconGlyph: null,      extraIconColor: null));

        Controls.Add(new Label
        {
            Text = "\"마음대로 가져다 쓰세요.\"",
            Font = new Font(TextFont, BaseFontSize, FontStyle.Italic),
            Location = new Point(RightX, QuoteY),
            Size = new Size(GithubWidth, 22),
            TextAlign = ContentAlignment.MiddleRight
        });
    }

    private static Control BuildCard(
        Point location, int width,
        string iconGlyph, Color iconColor,
        string label,
        string valueText,
        string? href,
        string? extraIconGlyph, Color? extraIconColor)
    {
        var card = new RoundedPanel
        {
            BackColor = Color.White,
            Location = location,
            Size = new Size(width, CardHeight)
        };

        card.Controls.Add(new Label
        {
            Text = iconGlyph,
            Font = new Font(IconFont, 18f),
            ForeColor = iconColor,
            Location = new Point(IconX, IconY),
            AutoSize = true
        });

        card.Controls.Add(new Label
        {
            Text = label,
            Font = new Font(TextFont, BaseFontSize, FontStyle.Bold),
            Location = new Point(LabelX, LabelY),
            AutoSize = true
        });

        Control valueControl = href != null
            ? CreateLink(valueText, href, new Point(ValueX, ValueY))
            : new Label
            {
                Text = valueText,
                Font = new Font(TextFont, BaseFontSize),
                Location = new Point(ValueX, ValueY),
                AutoSize = true
            };
        card.Controls.Add(valueControl);

        if (extraIconGlyph != null)
        {
            card.Controls.Add(new Label
            {
                Text = extraIconGlyph,
                Font = new Font(IconFont, ExtraIconFontSize),
                ForeColor = extraIconColor ?? Color.Black,
                Location = new Point(width - ExtraIconRightPad - ExtraIconSize, (CardHeight - ExtraIconSize) / 2),
                AutoSize = true
            });
        }

        return card;
    }

    private static LinkLabel CreateLink(string text, string href, Point location)
    {
        var link = new LinkLabel
        {
            Text = text,
            Font = new Font(TextFont, BaseFontSize),
            Location = location,
            LinkColor = LinkColor,
            ActiveLinkColor = LinkColor,
            LinkBehavior = LinkBehavior.HoverUnderline,
            AutoSize = true
        };
        link.LinkClicked += (_, _) => Process.Start(new ProcessStartInfo
        {
            FileName = href,
            UseShellExecute = true
        });
        return link;
    }
}
