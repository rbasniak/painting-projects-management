using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace PaintingProjectsManagement.Blazor.Modules.Authentication;

public static class JwtTokenHelper
{
    public static bool IsTokenExpired(string token)
    {
        if (string.IsNullOrEmpty(token))
            return true;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            
            return jsonToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            // If we can't parse the token, consider it expired
            return true;
        }
    }

    public static DateTime? GetTokenExpiration(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            
            return jsonToken.ValidTo;
        }
        catch
        {
            return null;
        }
    }

    public static bool IsTokenExpiringSoon(string token, TimeSpan threshold = default)
    {
        if (threshold == default)
            threshold = TimeSpan.FromMinutes(5); // Default 5 minutes threshold

        var expiration = GetTokenExpiration(token);
        if (!expiration.HasValue)
            return true;

        return expiration.Value - DateTime.UtcNow <= threshold;
    }
} 