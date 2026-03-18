namespace PaintingProjectsManagement.Features.Subscriptions.Web;

public interface ICurrentSubscriptionDetails
{
    SubscriptionTier Tier { get; }
    SubscriptionStatus Status { get; }
    DateTime? CurrentPeriodEndUtc { get; }
    bool CancelAtPeriodEnd { get; }
    decimal MonthlyPriceUsd { get; }
}
