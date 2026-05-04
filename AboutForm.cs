using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace HangulSwitcher;

internal sealed class AboutForm : Form
{
    public AboutForm()
    {
        Text = $"{AppInfo.AppName} 정보";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(20);
        Font = SystemFonts.MessageBoxFont!;

        Controls.Add(BuildLayout());
    }

    private TableLayoutPanel BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = AppInfo.Tagline,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 16)
        };
        layout.Controls.Add(title, 0, 0);
        layout.SetColumnSpan(title, 2);

        AddRow(layout, 1, "프로젝트", BuildLink(AppInfo.ProjectUrl, AppInfo.ProjectUrl));
        AddRow(layout, 2, "이메일",   BuildLink(AppInfo.ContactEmail, $"mailto:{AppInfo.ContactEmail}"));
        AddRow(layout, 3, "라이선스", new Label { Text = AppInfo.LicenseLine, AutoSize = true });

        var ok = new Button
        {
            Text = "확인",
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0, 16, 0, 0)
        };
        ok.Click += (_, _) => Close();
        layout.Controls.Add(ok, 1, 4);
        AcceptButton = ok;

        return layout;
    }

    private static void AddRow(TableLayoutPanel layout, int row, string label, Control value)
    {
        layout.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Margin = new Padding(0, 4, 16, 4)
        }, 0, row);
        value.Margin = new Padding(0, 4, 0, 4);
        layout.Controls.Add(value, 1, row);
    }

    private static LinkLabel BuildLink(string text, string href)
    {
        var link = new LinkLabel { Text = text, AutoSize = true };
        link.LinkClicked += (_, _) => Process.Start(new ProcessStartInfo
        {
            FileName = href,
            UseShellExecute = true
        });
        return link;
    }
}
