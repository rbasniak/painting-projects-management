namespace PaintingProjectsManagement.Features.Authorization;

public sealed record ProfileDetails
{
    public string Username { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Tenant { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
}
