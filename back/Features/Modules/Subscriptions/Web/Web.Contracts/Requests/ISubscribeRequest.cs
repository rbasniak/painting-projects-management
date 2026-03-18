namespace PaintingProjectsManagement.Features.Subscriptions.Web;

public interface ISubscribeRequest
{
    SubscriptionTier Tier { get; }
}

public interface IUpgradeSubscriptionRequest
{
    SubscriptionTier Tier { get; }
}

public interface ICancelSubscriptionRequest
{
    bool CancelAtPeriodEnd { get; }
}
