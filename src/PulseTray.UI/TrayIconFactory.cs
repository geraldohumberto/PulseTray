using PulseTray.Core;

namespace PulseTray.UI;

internal static class TrayIconFactory
{
    public static Icon Create(IReadOnlyCollection<QueryResult> results, bool hasError = false)
    {
        using var bitmap = new Bitmap(32, 32);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(hasError || results.Any(result => result.IsAlert) ? Color.FromArgb(190, 30, 45) : Color.FromArgb(34, 139, 84));
        var label = BuildLabel(results, hasError);
        using var font = CreateFittingFont(graphics, label);
        TextRenderer.DrawText(
            graphics,
            label,
            font,
            new Rectangle(0, -1, 32, 34),
            Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        return Icon.FromHandle(bitmap.GetHicon());
    }

    private static Font CreateFittingFont(Graphics graphics, string label)
    {
        for (var size = 31; size >= 9; size--)
        {
            var font = new Font("Segoe UI", size, FontStyle.Bold, GraphicsUnit.Pixel);
            var measured = TextRenderer.MeasureText(
                graphics,
                label,
                font,
                Size.Empty,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

            if (measured.Width <= 32 && measured.Height <= 34)
            {
                return font;
            }

            font.Dispose();
        }

        return new Font("Segoe UI", 9, FontStyle.Bold, GraphicsUnit.Pixel);
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
