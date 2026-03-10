using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Subscriptions.Integration;

using Request = PaintingProjectsManagement.Features.Subscriptions.Integration.GetSubscriptionEntitlementQuery;

namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed class GetSubscriptionEntitlement
{
    public sealed class Validator : AbstractValidator<Request>
    {
    }

    public sealed class Handler(DbContext context, ISubscriptionTierPolicyCatalog policyCatalog)
        : IQueryHandler<Request, SubscriptionEntitlementResult>
    {
        public async Task<QueryResponse<SubscriptionEntitlementResult>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var tenant = request.TenantId ?? string.Empty;
            var subscription = await context.Set<TenantSubscription>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenant, cancellationToken);

            var effectiveTier = SubscriptionTier.Free;
            var effectiveStatus = SubscriptionStatus.Active;
            var periodEnd = subscription?.CurrentPeriodEndUtc;
            var cancelAtPeriodEnd = subscription?.CancelAtPeriodEnd ?? false;

            if (subscription is not null)
            {
                effectiveTier = subscription.Tier;
                effectiveStatus = subscription.Status;

                var isExpired = subscription.Tier != SubscriptionTier.Free
                    && subscription.CurrentPeriodEndUtc.HasValue
                    && subscription.CurrentPeriodEndUtc.Value <= DateTime.UtcNow;

                if (isExpired)
                {
                    effectiveTier = SubscriptionTier.Free;
                    effectiveStatus = SubscriptionStatus.Expired;
                    cancelAtPeriodEnd = false;
                }
            }

            var policy = policyCatalog.Get(effectiveTier);

            var result = new SubscriptionEntitlementResult
            {
                Tier = effectiveTier,
                Status = effectiveStatus,
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

            return QueryResponse<SubscriptionEntitlementResult>.Success(result);
        }
    }
}
