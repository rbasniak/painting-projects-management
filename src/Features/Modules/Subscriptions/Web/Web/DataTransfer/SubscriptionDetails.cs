namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed class CurrentSubscriptionDetails : ICurrentSubscriptionDetails
{
    public SubscriptionTier Tier { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime? CurrentPeriodEndUtc { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public decimal MonthlyPriceUsd { get; set; }
}

public sealed class SubscriptionTierDetails : ISubscriptionTierDetails
{
    public SubscriptionTier Tier { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public decimal MonthlyPriceUsd { get; set; }
    public int MaxActiveProjects { get; set; }
    public int MaxInventoryPaints { get; set; }
    public int MaxModelPicturesPerModel { get; set; }
    public int MaxProjectReferencePicturesPerProject { get; set; }
    public int MaxProjectFinishedPicturesPerProject { get; set; }
    public long MaxStorageBytes { get; set; }
    public bool AllowHighResolutionImages { get; set; }
}
