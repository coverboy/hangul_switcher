using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HangulSwitcher;

internal sealed class AboutLogoPanel : Panel
{
    private const int CORNER_RADIUS = 16;
    private const float TEXT_EM_RATIO  = 0.46f;
    private const float CHAR_BOX_RATIO = 0.62f;
    private const float OFFSET_RATIO   = 1f - CHAR_BOX_RATIO;
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
        using var brush = new SolidBrush(Color.White);
        float emSize = side * TEXT_EM_RATIO;

        // "한" 좌상단
        DrawChar(g, brush, family, "한", emSize,
            new RectangleF(rect.X, rect.Y, rect.Width * CHAR_BOX_RATIO, rect.Height * CHAR_BOX_RATIO),
            StringAlignment.Near, StringAlignment.Near);

        // "영" 우하단 — "한" 과 일부 겹쳐서 대각선 느낌
        DrawChar(g, brush, family, "영", emSize,
            new RectangleF(
                rect.X + rect.Width  * OFFSET_RATIO,
                rect.Y + rect.Height * OFFSET_RATIO,
                rect.Width  * CHAR_BOX_RATIO,
                rect.Height * CHAR_BOX_RATIO),
            StringAlignment.Far, StringAlignment.Far);
    }

    private static void DrawChar(
        Graphics g, Brush brush, FontFamily family,
        string ch, float emSize, RectangleF box,
        StringAlignment hAlign, StringAlignment vAlign)
    {
        var format = new StringFormat
        {
            Alignment = hAlign,
            LineAlignment = vAlign
        };
        using var path = new GraphicsPath();
        path.AddString(ch, family, (int)FontStyle.Bold, emSize, box, format);
        g.FillPath(brush, path);
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
