namespace PaintingProjectsManagement.UI.Modules.Shared;

public class AuthenticationState
{
    private AuthenticationState()
    {
        this.IsAuthenticated = false;
        this.Username = string.Empty;
        this.Claims = (IReadOnlyCollection<string>)new List<string>();
    }

    public bool IsAuthenticated { get; set; }

    public string Username { get; set; }

    public IReadOnlyCollection<string> Claims { get; set; }

    public static AuthenticationState Anonymous()
    {
        return new AuthenticationState()
        {
            IsAuthenticated = false,
            Username = string.Empty,
            Claims = (IReadOnlyCollection<string>)new List<string>()
        };
    }

    public static AuthenticationState Authenticated(string username, IEnumerable<string> claims)
    {
        return new AuthenticationState()
        {
            IsAuthenticated = true,
            Username = username,
            Claims = (IReadOnlyCollection<string>)((claims != null ? claims.ToList<string>() : (List<string>)null) ?? new List<string>())
        };
    }
}
