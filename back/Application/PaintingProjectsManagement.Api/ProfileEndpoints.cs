using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Infrastructure.Common;
using PaintingProjectsManagment.Database;
using rbkApiModules.Identity.Core;
using System.Security.Claims;

namespace PaintingProjectsManagement.Api;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/profile/me", GetProfileAsync)
            .Produces<ProfileDetails>(StatusCodes.Status200OK)
            .RequireAuthorization()
            .WithName("Get Profile")
            .WithTags("Profile");

        app.MapGet("/api/profile/storage-usage", GetStorageUsageAsync)
            .Produces<StorageUsageDetails>(StatusCodes.Status200OK)
            .RequireAuthorization()
            .WithName("Get Storage Usage")
            .WithTags("Profile");

        return app;
    }

    private static async Task<IResult> GetProfileAsync(HttpContext httpContext, DatabaseContext context, CancellationToken cancellationToken)
    {
        var username = GetClaimValue(httpContext.User, ClaimTypes.Name, "username", "unique_name", "preferred_username", "sub");
        var tenant = GetClaimValue(httpContext.User, "tenant", "tenantId", "tenant_id", "tid");

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

        return Results.Ok(profile);
    }

    private static async Task<IResult> GetStorageUsageAsync(HttpContext httpContext, DatabaseContext context, ITenantStorageUsageService storageUsageService, CancellationToken cancellationToken)
    {
        var tenant = await ResolveTenantAsync(httpContext.User, context, cancellationToken);
        if (string.IsNullOrWhiteSpace(tenant))
        {
            return Results.Ok(new StorageUsageDetails
            {
                QuotaBytes = storageUsageService.QuotaInBytes,
                RemainingBytes = storageUsageService.QuotaInBytes
            });
        }

        var usedBytes = await storageUsageService.GetUsageInBytesAsync(tenant, cancellationToken);
        var usage = new StorageUsageDetails
        {
            UsedBytes = usedBytes,
            QuotaBytes = storageUsageService.QuotaInBytes,
            RemainingBytes = Math.Max(0, storageUsageService.QuotaInBytes - usedBytes)
        };

        return Results.Ok(usage);
    }

    private static async Task<string> ResolveTenantAsync(ClaimsPrincipal principal, DatabaseContext context, CancellationToken cancellationToken)
    {
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

    private static string? GetClaimValue(ClaimsPrincipal principal, params string[] claimTypes)
    {
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

public sealed record ProfileDetails
{
    public string Username { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Tenant { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
}

public sealed record StorageUsageDetails
{
    public long UsedBytes { get; init; }
    public long QuotaBytes { get; init; }
    public long RemainingBytes { get; init; }
}
