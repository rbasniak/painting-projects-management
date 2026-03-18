using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Infrastructure.Common;
using rbkApiModules.Identity.Core;

namespace PaintingProjectsManagement.Features.Authorization;

public class GetStorageUsage : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/profile/storage-usage", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<StorageUsageDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Get Storage Usage")
        .WithTags("Profile");
    }

    public class Request : AuthenticatedRequest, IQuery
    {
    }

    public class Validator : AbstractValidator<Request>
    {
    }

    public class Handler(
        DbContext context,
        IHttpContextAccessor httpContextAccessor,
        ITenantStorageUsageService storageUsageService) : IQueryHandler<Request>
    {
        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var principal = httpContextAccessor.HttpContext?.User;
            var tenant = await ResolveTenantAsync(request, principal, context, cancellationToken);

            if (string.IsNullOrWhiteSpace(tenant))
            {
                var defaultQuota = await storageUsageService.GetQuotaInBytesAsync(string.Empty, cancellationToken);
                return QueryResponse.Success(new StorageUsageDetails
                {
                    QuotaBytes = defaultQuota,
                    RemainingBytes = defaultQuota
                });
            }

            var usedBytes = await storageUsageService.GetUsageInBytesAsync(tenant, cancellationToken);
            var quotaBytes = await storageUsageService.GetQuotaInBytesAsync(tenant, cancellationToken);
            var result = new StorageUsageDetails
            {
                UsedBytes = usedBytes,
                QuotaBytes = quotaBytes,
                RemainingBytes = Math.Max(0, quotaBytes - usedBytes)
            };

            return QueryResponse.Success(result);
        }

        private static async Task<string> ResolveTenantAsync(Request request, ClaimsPrincipal? principal, DbContext context, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.Identity.Tenant))
            {
                return request.Identity.Tenant;
            }

            var tenant = GetClaimValue(principal, "tenant", "tenantId", "tenant_id", "tid");
            if (!string.IsNullOrWhiteSpace(tenant))
            {
                return tenant;
            }

            var username = GetClaimValue(principal, ClaimTypes.Name, "username", "unique_name", "preferred_username", "sub");
            if (string.IsNullOrWhiteSpace(username))
            {
                return string.Empty;
            }

            return await context.Set<User>()
                .AsNoTracking()
                .Where(x => x.Username == username)
                .Select(x => x.TenantId)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
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
