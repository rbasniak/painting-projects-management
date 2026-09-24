namespace PaintingProjectsManagement.Features.Subscriptions.Web;

public interface ISubscriptionTierDetails
{
    SubscriptionTier Tier { get; }
    string DisplayName { get; }
    decimal MonthlyPriceUsd { get; }
    int MaxActiveProjects { get; }
    int MaxInventoryPaints { get; }
    int MaxModelPicturesPerModel { get; }
    int MaxProjectReferencePicturesPerProject { get; }
    int MaxProjectFinishedPicturesPerProject { get; }
    long MaxStorageBytes { get; }
    bool AllowHighResolutionImages { get; }
}
