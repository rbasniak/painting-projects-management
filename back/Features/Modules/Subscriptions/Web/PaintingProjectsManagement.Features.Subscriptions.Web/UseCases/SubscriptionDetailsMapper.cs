namespace PaintingProjectsManagement.Features.Subscriptions;

internal static class SubscriptionDetailsMapper
{
    public static CurrentSubscriptionDetails ToCurrentDetails(
        SubscriptionEntitlementResult entitlement,
        ISubscriptionTierPolicyCatalog policyCatalog)
    {
        var policy = policyCatalog.Get(entitlement.Tier);
        return new CurrentSubscriptionDetails
        {
            Tier = entitlement.Tier,
            Status = entitlement.Status,
            CurrentPeriodEndUtc = entitlement.CurrentPeriodEndUtc,
            CancelAtPeriodEnd = entitlement.CancelAtPeriodEnd,
            MonthlyPriceUsd = policy.MonthlyPriceUsd
        };
    }
}
