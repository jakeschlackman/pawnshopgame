namespace PawnShop.Framework.UI;

// Fixed-width wrapping for the prototype pixel font. Long words cannot overflow.
public static class TextPages
{
    public static IReadOnlyList<string> Wrap(string text, int columns)
    {
        if (columns < 1) throw new ArgumentOutOfRangeException(nameof(columns));
        var lines = new List<string>();
        foreach (var paragraph in text.Replace("\r", "").Split('\n'))
        {
            string remaining = paragraph.Trim();
            while (remaining.Length > columns)
            {
                int end = remaining.LastIndexOf(' ', columns, columns + 1);
                if (end <= 0) end = columns;
                lines.Add(remaining[..end]);
                remaining = remaining[end..].TrimStart();
            }
            lines.Add(remaining);
        }
        return lines.AsReadOnly();
    }
}
