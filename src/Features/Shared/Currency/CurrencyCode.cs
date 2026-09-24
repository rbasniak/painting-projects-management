namespace PaintingProjectsManagement.Features.Currency;

/// <summary>
/// Normalizes currency codes from query strings, UI, or legacy DB values (BOM, zero-width chars, "EURO", etc.).
/// </summary>
public static class CurrencyCode
{
    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        Span<char> buf = stackalloc char[3];
        var n = 0;
        foreach (var ch in raw.AsSpan().Trim())
        {
            if (ch is '\uFEFF' or '\u200B' or '\u200C' or '\u200D')
            {
                continue;
            }

            if (n >= 3)
            {
                break;
            }

            if (ch is >= 'A' and <= 'Z')
            {
                buf[n++] = ch;
            }
            else if (ch is >= 'a' and <= 'z')
            {
                buf[n++] = (char)(ch - 32);
            }
        }

        if (n == 3)
        {
            return new string(buf);
        }

        if (n > 0)
        {
            return new string(buf[..n]);
        }

        return raw.Trim().ToUpperInvariant();
    }
}
