using PulseTray.Core;

namespace PulseTray.UI;

internal static class TrayIconFactory
{
    public static Icon Create(IReadOnlyCollection<QueryResult> results, bool hasError = false)
    {
        using var bitmap = new Bitmap(32, 32);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(hasError || results.Any(result => result.IsAlert) ? Color.FromArgb(190, 30, 45) : Color.FromArgb(34, 139, 84));
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var label = BuildLabel(results, hasError);
        using var font = new Font("Segoe UI", label.Length > 2 ? 9 : 12, FontStyle.Bold, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(Color.White);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        graphics.DrawString(label, font, brush, new RectangleF(0, 0, 32, 32), format);
        return Icon.FromHandle(bitmap.GetHicon());
    }

    private static string BuildLabel(IReadOnlyCollection<QueryResult> results, bool hasError)
    {
        if (hasError)
        {
            return "!";
        }

        if (results.Count == 0)
        {
            return "-";
        }

        if (results.Count == 1)
        {
            return ShortNumber(results.First().Value);
        }

        return string.Join(" ", results.Take(3).Select(result => ShortNumber(result.Value)));
    }

    private static string ShortNumber(long value)
    {
        if (value >= 1000)
        {
            return $"{value / 1000}k";
        }

        return value.ToString();
    }
}
