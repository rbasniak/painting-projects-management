using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using rbkApiModules.Identity.Core;

namespace PaintingProjectsManagement.Features.Authorization;

public class GetProfile : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/profile/me", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
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

    public class Handler(DbContext context, IHttpContextAccessor httpContextAccessor) : IQueryHandler<Request>
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

            var profile = new ProfileDetails
            {
                Username = user?.Username ?? username ?? string.Empty,
                DisplayName = user?.DisplayName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                Avatar = user?.Avatar ?? string.Empty,
                Tenant = user?.TenantId ?? tenant ?? string.Empty
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
