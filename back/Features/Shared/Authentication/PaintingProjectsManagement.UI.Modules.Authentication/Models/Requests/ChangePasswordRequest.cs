namespace PaintingProjectsManagement.Blazor.Modules.Authentication;

public sealed record ChangePasswordRequest
{
    public string OldPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string PasswordConfirmation { get; init; } = string.Empty;
}
