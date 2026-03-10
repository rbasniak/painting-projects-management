namespace PaintingProjectsManagement.Features.Subscriptions;

public enum SubscriptionPaymentStatus
{
    Succeeded = 0,
    Failed = 1,
    Refunded = 2
}

public sealed class SubscriptionPayment : TenantEntity
{
    private SubscriptionPayment()
    {
    }

    public SubscriptionPayment(
        string tenantId,
        Guid subscriptionId,
        SubscriptionTier tierAtPayment,
        decimal amount,
        string currency,
        string provider,
        string providerTransactionId,
        SubscriptionPaymentStatus status,
        DateTime? billingPeriodStartUtc,
        DateTime? billingPeriodEndUtc,
        string? failureReason = null)
    {
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        TierAtPayment = tierAtPayment;
        Amount = amount;
        Currency = currency;
        Provider = provider;
        ProviderTransactionId = providerTransactionId;
        Status = status;
        FailureReason = failureReason ?? string.Empty;
        BillingPeriodStartUtc = billingPeriodStartUtc;
        BillingPeriodEndUtc = billingPeriodEndUtc;
        ProcessedUtc = DateTime.UtcNow;
        CreatedUtc = DateTime.UtcNow;
    }

    public Guid SubscriptionId { get; private set; }
    public SubscriptionTier TierAtPayment { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string ProviderTransactionId { get; private set; } = string.Empty;
    public SubscriptionPaymentStatus Status { get; private set; }
    public string FailureReason { get; private set; } = string.Empty;
    public DateTime? BillingPeriodStartUtc { get; private set; }
    public DateTime? BillingPeriodEndUtc { get; private set; }
    public DateTime ProcessedUtc { get; private set; }
    public DateTime CreatedUtc { get; private set; }
}
