using System.Security.Claims;

namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed class GetCurrentSubscription : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/subscriptions/me", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<CurrentSubscriptionDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Get Current Subscription")
        .WithTags("Subscriptions");
    }

    public sealed class Request : AuthenticatedRequest, IQuery
    {
    }

    public sealed class Validator : AbstractValidator<Request>
    {
    }

    public sealed class Handler(
        IHttpContextAccessor httpContextAccessor,
        ISubscriptionAccessService subscriptionAccessService,
        ISubscriptionTierPolicyCatalog policyCatalog) : IQueryHandler<Request>
    {
        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var principal = httpContextAccessor.HttpContext?.User;
            var tenant = request.Identity.Tenant
                ?? GetClaimValue(principal, "tenant", "tenantId", "tenant_id", "tid")
                ?? GetClaimValue(principal, ClaimTypes.Name, "username", "preferred_username", "sub")
                ?? string.Empty;
            var entitlement = await subscriptionAccessService.ResolveEntitlementAsync(tenant, cancellationToken);
            return QueryResponse.Success(SubscriptionDetailsMapper.ToCurrentDetails(entitlement, policyCatalog));
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
