using System.Globalization;
using System.Text;

namespace Desaka.Comparation;

/// <summary>
/// Utilities for text normalization, especially for Czech/Slovak text.
/// </summary>
public static class TextNormalizer
{
    private static readonly Dictionary<char, char> DiacriticsMap = new()
    {
        ['á'] = 'a', ['Á'] = 'A', ['č'] = 'c', ['Č'] = 'C',
        ['ď'] = 'd', ['Ď'] = 'D', ['é'] = 'e', ['É'] = 'E',
        ['ě'] = 'e', ['Ě'] = 'E', ['í'] = 'i', ['Í'] = 'I',
        ['ň'] = 'n', ['Ň'] = 'N', ['ó'] = 'o', ['Ó'] = 'O',
        ['ô'] = 'o', ['Ô'] = 'O', ['ř'] = 'r', ['Ř'] = 'R',
        ['š'] = 's', ['Š'] = 'S', ['ť'] = 't', ['Ť'] = 'T',
        ['ú'] = 'u', ['Ú'] = 'U', ['ů'] = 'u', ['Ů'] = 'U',
        ['ý'] = 'y', ['Ý'] = 'Y', ['ž'] = 'z', ['Ž'] = 'Z',
        ['ä'] = 'a', ['Ä'] = 'A', ['ĺ'] = 'l', ['Ĺ'] = 'L',
        ['ľ'] = 'l', ['Ľ'] = 'L', ['ŕ'] = 'r', ['Ŕ'] = 'R',
        ['ö'] = 'o', ['Ö'] = 'O', ['ü'] = 'u', ['Ü'] = 'U', ['ß'] = 's',
    };

    public static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (DiacriticsMap.TryGetValue(c, out var replacement))
                sb.Append(replacement);
            else if (c > 127)
            {
                var normalized = c.ToString().Normalize(NormalizationForm.FormD);
                foreach (var nc in normalized)
                    if (CharUnicodeInfo.GetUnicodeCategory(nc) != UnicodeCategory.NonSpacingMark)
                        sb.Append(nc);
            }
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    public static string NormalizeWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var sb = new StringBuilder(text.Length);
        var lastWasSpace = true;
        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace) { sb.Append(' '); lastWasSpace = true; }
            }
            else { sb.Append(c); lastWasSpace = false; }
        }
        if (sb.Length > 0 && sb[^1] == ' ') sb.Length--;
        return sb.ToString();
    }

    public static string ToSearchForm(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return RemoveDiacritics(NormalizeWhitespace(text)).ToLowerInvariant();
    }
}
