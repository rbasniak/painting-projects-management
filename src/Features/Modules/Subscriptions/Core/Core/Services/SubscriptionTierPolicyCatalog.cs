namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed record SubscriptionTierPolicy
{
    public required SubscriptionTier Tier { get; init; }
    public required string DisplayName { get; init; }
    public required decimal MonthlyPriceUsd { get; init; }
    public required int MaxActiveProjects { get; init; }
    public required int MaxInventoryPaints { get; init; }
    public required int MaxModelPicturesPerModel { get; init; }
    public required int MaxProjectReferencePicturesPerProject { get; init; }
    public required int MaxProjectFinishedPicturesPerProject { get; init; }
    public required long MaxStorageBytes { get; init; }
    public required bool AllowHighResolutionImages { get; init; }
}

public interface ISubscriptionTierPolicyCatalog
{
    SubscriptionTierPolicy Get(SubscriptionTier tier);
    IReadOnlyCollection<SubscriptionTierPolicy> List();
}

public sealed class SubscriptionTierPolicyCatalog : ISubscriptionTierPolicyCatalog
{
    public const int Unlimited = int.MaxValue;
    public const long Mb = 1024L * 1024L;

    private static readonly IReadOnlyDictionary<SubscriptionTier, SubscriptionTierPolicy> Policies =
        new Dictionary<SubscriptionTier, SubscriptionTierPolicy>
        {
            [SubscriptionTier.Free] = new SubscriptionTierPolicy
            {
                Tier = SubscriptionTier.Free,
                DisplayName = "Free",
                MonthlyPriceUsd = 0m,
                MaxActiveProjects = 2,
                MaxInventoryPaints = 50,
                MaxModelPicturesPerModel = 3,
                MaxProjectReferencePicturesPerProject = 3,
                MaxProjectFinishedPicturesPerProject = 3,
                MaxStorageBytes = 100 * Mb,
                AllowHighResolutionImages = false
            },
            [SubscriptionTier.Basic] = new SubscriptionTierPolicy
            {
                Tier = SubscriptionTier.Basic,
                DisplayName = "Basic",
                MonthlyPriceUsd = 9m,
                MaxActiveProjects = Unlimited,
                MaxInventoryPaints = Unlimited,
                MaxModelPicturesPerModel = 50,
                MaxProjectReferencePicturesPerProject = 50,
                MaxProjectFinishedPicturesPerProject = 50,
                MaxStorageBytes = 500 * Mb,
                AllowHighResolutionImages = true
            },
            [SubscriptionTier.Premium] = new SubscriptionTierPolicy
            {
                Tier = SubscriptionTier.Premium,
                DisplayName = "Premium",
                MonthlyPriceUsd = 29m,
                MaxActiveProjects = Unlimited,
                MaxInventoryPaints = Unlimited,
                MaxModelPicturesPerModel = 250,
                MaxProjectReferencePicturesPerProject = 250,
                MaxProjectFinishedPicturesPerProject = 250,
                MaxStorageBytes = 2_000 * Mb,
                AllowHighResolutionImages = true
            }
        };

    public SubscriptionTierPolicy Get(SubscriptionTier tier)
    {
        return Policies.TryGetValue(tier, out var policy)
            ? policy
            : Policies[SubscriptionTier.Free];
    }

    public IReadOnlyCollection<SubscriptionTierPolicy> List() => Policies.Values.ToArray();
}
