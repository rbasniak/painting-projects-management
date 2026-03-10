using System;

namespace PaintingProjectsManagement.Features.Subscriptions.Integration;

public sealed class SubscriptionEntitlementResult
{
    public SubscriptionTier Tier { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime? CurrentPeriodEndUtc { get; set; }
    public bool CancelAtPeriodEnd { get; set; }

    public int MaxActiveProjects { get; set; }
    public int MaxInventoryPaints { get; set; }
    public int MaxModelPicturesPerModel { get; set; }
    public int MaxProjectReferencePicturesPerProject { get; set; }
    public int MaxProjectFinishedPicturesPerProject { get; set; }
    public long MaxStorageBytes { get; set; }
    public bool AllowHighResolutionImages { get; set; }
}
