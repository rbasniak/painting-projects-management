using PaintingProjectsManagement.Features.Subscriptions;

namespace PaintingProjectsManagement.Blazor.Modules.Authentication;

public sealed record CurrentSubscriptionSnapshotResponse
{
    public SubscriptionTier Tier { get; init; } = SubscriptionTier.Free;
    public SubscriptionStatus Status { get; init; } = SubscriptionStatus.Active;
    public DateTime? CurrentPeriodEndUtc { get; init; }
    public bool CancelAtPeriodEnd { get; init; }
}
