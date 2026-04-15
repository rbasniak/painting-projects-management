namespace PaintingProjectsManagement.Features.Subscriptions;

public enum SubscriptionTier
{
    Free = 0,
    Basic = 1,
    Premium = 2
}

public enum SubscriptionStatus
{
    Active = 0,
    Cancelled = 1,
    Expired = 2,
    PendingPayment = 3,
    PastDue = 4
}
