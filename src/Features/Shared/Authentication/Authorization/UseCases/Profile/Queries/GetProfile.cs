using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Subscriptions;
using PaintingProjectsManagement.Features.Subscriptions.Integration;
using rbkApiModules.Identity.Core;

namespace PaintingProjectsManagement.Features.Authorization;

public class GetProfile : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/profile/me", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<ProfileDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Get Profile")
        .WithTags("Profile");
    }

    public class Request : AuthenticatedRequest, IQuery
    {
    }

    public class Validator : AbstractValidator<Request>
    {
    }

    public class Handler(DbContext context, IHttpContextAccessor httpContextAccessor, IDispatcher dispatcher) : IQueryHandler<Request>
    {
        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var principal = httpContextAccessor.HttpContext?.User;
            var username = GetClaimValue(principal, ClaimTypes.Name, "username", "unique_name", "preferred_username", "sub");
            var tenant = !string.IsNullOrWhiteSpace(request.Identity.Tenant)
                ? request.Identity.Tenant
                : GetClaimValue(principal, "tenant", "tenantId", "tenant_id", "tid");

            User? user = null;
            if (!string.IsNullOrWhiteSpace(username))
            {
                user = await context.Set<User>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
            }

            if (user is null && !string.IsNullOrWhiteSpace(tenant))
            {
                user = await context.Set<User>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenant, cancellationToken);
            }

            var resolvedTenant = user?.TenantId ?? tenant ?? string.Empty;
            var subscriptionTier = SubscriptionTier.Free;
            var subscriptionStatus = SubscriptionStatus.Active;
            DateTime? subscriptionPeriodEndUtc = null;
            var subscriptionCancelAtPeriodEnd = false;

            var primaryTenant = request.Identity.Tenant;
            var secondaryTenant = resolvedTenant;

            var primaryEntitlement = await dispatcher.SendAsync(
                new GetSubscriptionEntitlementQuery { TenantId = primaryTenant },
                cancellationToken);
            if (primaryEntitlement.IsValid && primaryEntitlement.Data is not null)
            {
                subscriptionTier = primaryEntitlement.Data.Tier;
                subscriptionStatus = primaryEntitlement.Data.Status;
                subscriptionPeriodEndUtc = primaryEntitlement.Data.CurrentPeriodEndUtc;
                subscriptionCancelAtPeriodEnd = primaryEntitlement.Data.CancelAtPeriodEnd;
            }

            var shouldTrySecondary = !string.IsNullOrWhiteSpace(secondaryTenant)
                && !string.Equals(primaryTenant, secondaryTenant, StringComparison.OrdinalIgnoreCase)
                && subscriptionTier == SubscriptionTier.Free;

            if (shouldTrySecondary)
            {
                var secondaryEntitlement = await dispatcher.SendAsync(
                    new GetSubscriptionEntitlementQuery { TenantId = secondaryTenant },
                    cancellationToken);
                if (secondaryEntitlement.IsValid && secondaryEntitlement.Data is not null)
                {
                    subscriptionTier = secondaryEntitlement.Data.Tier;
                    subscriptionStatus = secondaryEntitlement.Data.Status;
                    subscriptionPeriodEndUtc = secondaryEntitlement.Data.CurrentPeriodEndUtc;
                    subscriptionCancelAtPeriodEnd = secondaryEntitlement.Data.CancelAtPeriodEnd;
                }
            }

            var profile = new ProfileDetails
            {
                Username = user?.Username ?? username ?? string.Empty,
                DisplayName = user?.DisplayName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                Avatar = user?.Avatar ?? string.Empty,
                Tenant = resolvedTenant,
                SubscriptionTier = subscriptionTier,
                SubscriptionStatus = subscriptionStatus,
                SubscriptionPeriodEndUtc = subscriptionPeriodEndUtc,
                SubscriptionCancelAtPeriodEnd = subscriptionCancelAtPeriodEnd
            };

            return QueryResponse.Success(profile);
        }

        private static string? GetClaimValue(ClaimsPrincipal? principal, params string[] claimTypes)
        {
            if (principal is null)
            {
                return null;
            }

            foreach (var claimType in claimTypes)
            {
                var value = principal.FindFirst(claimType)?.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }
    }
}
