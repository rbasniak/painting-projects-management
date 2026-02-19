using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.UI.Modules.Shared;

public class AuthenticationState
{
    public bool IsAuthenticated { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiration { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [JsonIgnore]
    public bool IsTokenExpired => TokenExpiration.HasValue && TokenExpiration.Value <= DateTime.UtcNow;

    [JsonIgnore]
    public bool IsTokenExpiringSoon => TokenExpiration.HasValue && 
                                      TokenExpiration.Value <= DateTime.UtcNow.AddMinutes(5);
} 