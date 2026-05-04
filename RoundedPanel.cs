using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HangulSwitcher;

internal sealed class RoundedPanel : Panel
{
    public int CornerRadius { get; set; } = 8;
    public Color BorderColor { get; set; } = Color.FromArgb(220, 224, 230);

    public RoundedPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = ClientRectangle;
        rect.Width  -= 1;
        rect.Height -= 1;

        using var path = BuildRoundedPath(rect, CornerRadius);
        using var fill = new SolidBrush(BackColor);
        g.FillPath(fill, path);
        using var pen = new Pen(BorderColor);
        g.DrawPath(pen, path);
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
