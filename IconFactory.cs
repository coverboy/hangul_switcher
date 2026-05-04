using System.Drawing;
using System.Drawing.Drawing2D;

namespace HangulSwitcher;

internal static class IconFactory
{
    private const int ICON_SIZE = 32;
    private const string FONT_FAMILY = "Segoe UI";

    public static Icon CreateTextIcon(string text, Color foreground, Color background)
    {
        using var bmp = new Bitmap(ICON_SIZE, ICON_SIZE);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(background);

            // 글자 외곽선 path 를 만들고 실제 ink bounds 기준으로
            // ICON_SIZE 박스에 정확히 fit (font ascent/descent 여백 제거).
            using var family = new FontFamily(FONT_FAMILY);
            using var path = new GraphicsPath();
            path.AddString(
                text,
                family,
                (int)FontStyle.Bold,
                emSize: ICON_SIZE,
                origin: PointF.Empty,
                StringFormat.GenericTypographic);

            var bounds = path.GetBounds();
            if (bounds.Width > 0 && bounds.Height > 0)
            {
                var scale = Math.Min(ICON_SIZE / bounds.Width, ICON_SIZE / bounds.Height);
                using var matrix = new Matrix();
                matrix.Translate(-bounds.X, -bounds.Y);
                matrix.Scale(scale, scale, MatrixOrder.Append);
                matrix.Translate(
                    (ICON_SIZE - bounds.Width * scale) / 2f,
                    (ICON_SIZE - bounds.Height * scale) / 2f,
                    MatrixOrder.Append);
                path.Transform(matrix);
            }

            using var brush = new SolidBrush(foreground);
            g.FillPath(brush, path);
        }

        var hIcon = bmp.GetHicon();
        try
        {
            return (Icon)Icon.FromHandle(hIcon).Clone();
        }
        finally
        {
            NativeMethods.DestroyIcon(hIcon);
        }
    }
}
