namespace PaintingProjectsManagement.Features.Subscriptions;

public interface ISubscriptionAccessService
{
    Task<TenantSubscription> GetOrCreateAsync(string tenantId, CancellationToken cancellationToken);
    Task<SubscriptionEntitlementResult> ResolveEntitlementAsync(string tenantId, CancellationToken cancellationToken);
}

public sealed class SubscriptionAccessService(DbContext context, ISubscriptionTierPolicyCatalog policyCatalog)
    : ISubscriptionAccessService
{
    public async Task<TenantSubscription> GetOrCreateAsync(string tenantId, CancellationToken cancellationToken)
    {
        var subscription = await context.Set<TenantSubscription>()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (subscription is not null)
        {
            return subscription;
        }

        subscription = TenantSubscription.CreateFree(tenantId);
        await context.AddAsync(subscription, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task<SubscriptionEntitlementResult> ResolveEntitlementAsync(string tenantId, CancellationToken cancellationToken)
    {
        var subscription = await context.Set<TenantSubscription>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var tier = subscription?.Tier ?? SubscriptionTier.Free;
        var status = subscription?.Status ?? SubscriptionStatus.Active;
        var periodEnd = subscription?.CurrentPeriodEndUtc;
        var cancelAtPeriodEnd = subscription?.CancelAtPeriodEnd ?? false;

        var isExpired = subscription is not null
            && subscription.Tier != SubscriptionTier.Free
            && subscription.CurrentPeriodEndUtc.HasValue
            && subscription.CurrentPeriodEndUtc.Value <= DateTime.UtcNow;

        if (isExpired)
        {
            tier = SubscriptionTier.Free;
            status = SubscriptionStatus.Expired;
            cancelAtPeriodEnd = false;
        }

        var policy = policyCatalog.Get(tier);
        return new SubscriptionEntitlementResult
        {
            Tier = tier,
            Status = status,
            CurrentPeriodEndUtc = periodEnd,
            CancelAtPeriodEnd = cancelAtPeriodEnd,
            MaxActiveProjects = policy.MaxActiveProjects,
            MaxInventoryPaints = policy.MaxInventoryPaints,
            MaxModelPicturesPerModel = policy.MaxModelPicturesPerModel,
            MaxProjectReferencePicturesPerProject = policy.MaxProjectReferencePicturesPerProject,
            MaxProjectFinishedPicturesPerProject = policy.MaxProjectFinishedPicturesPerProject,
            MaxStorageBytes = policy.MaxStorageBytes,
            AllowHighResolutionImages = policy.AllowHighResolutionImages
        };
    }
}
