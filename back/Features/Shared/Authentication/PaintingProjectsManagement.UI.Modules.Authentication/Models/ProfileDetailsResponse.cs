using PaintingProjectsManagement.Features.Subscriptions;

namespace PaintingProjectsManagement.Blazor.Modules.Authentication;

public sealed record ProfileDetailsResponse
{
    public string Username { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Tenant { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public SubscriptionTier SubscriptionTier { get; init; } = SubscriptionTier.Free;
    public SubscriptionStatus SubscriptionStatus { get; init; } = SubscriptionStatus.Active;
    public DateTime? SubscriptionPeriodEndUtc { get; init; }
    public bool SubscriptionCancelAtPeriodEnd { get; init; }
}
