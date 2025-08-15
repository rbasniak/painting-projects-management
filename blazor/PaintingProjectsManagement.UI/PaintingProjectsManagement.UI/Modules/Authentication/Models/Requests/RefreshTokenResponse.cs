namespace PaintingProjectsManagement.Blazor.Modules.Authentication;

public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
