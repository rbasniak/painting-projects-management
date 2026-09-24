namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed class TenantSubscription : TenantEntity
{
    private TenantSubscription()
    {
    }

    private TenantSubscription(string tenantId)
    {
        TenantId = tenantId;
        Tier = SubscriptionTier.Free;
        Status = SubscriptionStatus.Active;
        AutoRenew = false;
        CancelAtPeriodEnd = false;
        CreatedUtc = DateTime.UtcNow;
        UpdatedUtc = DateTime.UtcNow;
    }

    public SubscriptionTier Tier { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime? CurrentPeriodStartUtc { get; private set; }
    public DateTime? CurrentPeriodEndUtc { get; private set; }
    public bool AutoRenew { get; private set; }
    public bool CancelAtPeriodEnd { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? ExpiredAtUtc { get; private set; }
    public string LastPaymentTransactionId { get; private set; } = string.Empty;
    public DateTime CreatedUtc { get; private set; }
    public DateTime UpdatedUtc { get; private set; }

    public static TenantSubscription CreateFree(string tenantId) => new(tenantId);

    public void ActivatePaidTier(SubscriptionTier tier, DateTime periodStartUtc, DateTime periodEndUtc, string paymentTransactionId)
    {
        if (tier == SubscriptionTier.Free)
        {
            throw new InvalidOperationException("Paid activation requires a non-free tier.");
        }

        Tier = tier;
        Status = SubscriptionStatus.Active;
        CurrentPeriodStartUtc = periodStartUtc;
        CurrentPeriodEndUtc = periodEndUtc;
        AutoRenew = true;
        CancelAtPeriodEnd = false;
        CancelledAtUtc = null;
        ExpiredAtUtc = null;
        LastPaymentTransactionId = paymentTransactionId;
        UpdatedUtc = DateTime.UtcNow;
    }

    public void MarkCancelAtPeriodEnd()
    {
        if (Tier == SubscriptionTier.Free)
        {
            return;
        }

        CancelAtPeriodEnd = true;
        AutoRenew = false;
        UpdatedUtc = DateTime.UtcNow;
    }

    public void CancelImmediately(DateTime cancelledAtUtc)
    {
        Tier = SubscriptionTier.Free;
        Status = SubscriptionStatus.Cancelled;
        CurrentPeriodEndUtc = cancelledAtUtc;
        AutoRenew = false;
        CancelAtPeriodEnd = false;
        CancelledAtUtc = cancelledAtUtc;
        UpdatedUtc = DateTime.UtcNow;
    }

    public void Expire(DateTime expiredAtUtc)
    {
        Tier = SubscriptionTier.Free;
        Status = SubscriptionStatus.Expired;
        AutoRenew = false;
        CancelAtPeriodEnd = false;
        ExpiredAtUtc = expiredAtUtc;
        UpdatedUtc = DateTime.UtcNow;
    }
}
