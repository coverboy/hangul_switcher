using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HangulSwitcher;

internal sealed class AboutLogoPanel : Panel
{
    private const int CORNER_RADIUS = 16;
    private const float TEXT_EM_RATIO = 0.32f;
    private static readonly Color LogoBackground = Color.FromArgb(50, 100, 200);

    public AboutLogoPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        int side = Math.Min(Width, Height);
        var rect = new Rectangle((Width - side) / 2, (Height - side) / 2, side - 1, side - 1);

        using var bgPath = BuildRoundedPath(rect, CORNER_RADIUS);
        using var fill = new SolidBrush(LogoBackground);
        g.FillPath(fill, bgPath);

        using var family = new FontFamily("Segoe UI");
        using var textPath = new GraphicsPath();
        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        textPath.AddString("한\n영", family, (int)FontStyle.Bold, side * TEXT_EM_RATIO, rect, format);
        using var brush = new SolidBrush(Color.White);
        g.FillPath(brush, textPath);
    }

    private static GraphicsPath BuildRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
