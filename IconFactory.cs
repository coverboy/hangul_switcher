using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace HangulSwitcher;

internal static class IconFactory
{
    private const int ICON_SIZE = 32;

    public static Icon CreateTextIcon(string text, Color foreground, Color background)
    {
        using var bmp = new Bitmap(ICON_SIZE, ICON_SIZE);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(background);

            var fontSize = text.Length == 1 ? 16f : 12f;
            using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(foreground);
            var size = g.MeasureString(text, font);
            var x = (ICON_SIZE - size.Width) / 2f;
            var y = (ICON_SIZE - size.Height) / 2f;
            g.DrawString(text, font, brush, x, y);
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
