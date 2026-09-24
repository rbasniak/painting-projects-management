using PaintingProjectsManagement.Features.Subscriptions;

namespace PaintingProjectsManagement.UI.Modules.Subscriptions;

public sealed class CurrentSubscriptionResponse
{
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime? CurrentPeriodEndUtc { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public decimal MonthlyPriceUsd { get; set; }
}

public sealed class SubscriptionTierResponse
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
