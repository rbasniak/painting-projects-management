using System.Text;

namespace PaintingProjectsManagement.Features.Models;

internal static class PublicModelsOwnerKeyCodec
{
    public static string Encode(string? owner)
    {
        var normalizedOwner = owner?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedOwner))
        {
            return string.Empty;
        }

        var bytes = Encoding.UTF8.GetBytes(normalizedOwner);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static string DecodeOrFallback(string? ownerKey)
    {
        var value = ownerKey?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (!TryDecode(value, out var decoded))
        {
            return value;
        }

        return decoded;
    }

    private static bool TryDecode(string ownerKey, out string decoded)
    {
        decoded = string.Empty;

        var base64 = ownerKey
            .Replace('-', '+')
            .Replace('_', '/');

        var remainder = base64.Length % 4;
        if (remainder > 0)
        {
            base64 = base64.PadRight(base64.Length + (4 - remainder), '=');
        }

        try
        {
            var bytes = Convert.FromBase64String(base64);
            decoded = Encoding.UTF8.GetString(bytes).Trim().ToLowerInvariant();
            return !string.IsNullOrWhiteSpace(decoded);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
