using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Models;
using PaintingProjectsManagement.Features.Projects;
using PaintingProjectsManagment.Database;
using rbkApiModules.Identity.Core;
using System.Security.Claims;

namespace PaintingProjectsManagement.Api;

public static class ProfileEndpoints
{
    public const long StorageQuotaInBytes = 100L * 1024 * 1024;

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

    private static async Task<IResult> GetStorageUsageAsync(HttpContext httpContext, DatabaseContext context, IWebHostEnvironment environment, CancellationToken cancellationToken)
    {
        var tenant = await ResolveTenantAsync(httpContext.User, context, cancellationToken);
        if (string.IsNullOrWhiteSpace(tenant))
        {
            return Results.Ok(new StorageUsageDetails());
        }

        var models = await context.Set<Model>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenant)
            .Select(x => new { x.CoverPicture, x.Pictures })
            .ToListAsync(cancellationToken);

        var projects = await context.Set<Project>()
            .AsNoTracking()
            .Include(x => x.References)
            .Include(x => x.Pictures)
            .Where(x => x.TenantId == tenant)
            .ToListAsync(cancellationToken);

        var urls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var model in models)
        {
            AddUrl(urls, model.CoverPicture);

            foreach (var picture in model.Pictures)
            {
                AddUrl(urls, picture);
            }
        }

        foreach (var project in projects)
        {
            AddUrl(urls, project.PictureUrl);

            foreach (var reference in project.References)
            {
                AddUrl(urls, reference.Url);
            }

            foreach (var picture in project.Pictures)
            {
                AddUrl(urls, picture.Url);
            }
        }

        long usedBytes = urls.Sum(url => TryGetFileSize(url, environment));
        var usage = new StorageUsageDetails
        {
            UsedBytes = usedBytes,
            QuotaBytes = StorageQuotaInBytes,
            RemainingBytes = Math.Max(0, StorageQuotaInBytes - usedBytes)
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

    private static void AddUrl(ICollection<string> target, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        target.Add(value);
    }

    private static long TryGetFileSize(string rawUrl, IWebHostEnvironment environment)
    {
        var relativePath = NormalizeRelativePath(rawUrl);
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return 0;
        }

        var roots = new[]
        {
            environment.WebRootPath,
            Path.Combine(environment.ContentRootPath, "wwwroot"),
            Path.Combine(AppContext.BaseDirectory, "wwwroot")
        }
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Select(Path.GetFullPath)
        .Distinct()
        .ToArray();

        foreach (var root in roots)
        {
            var candidate = Path.GetFullPath(Path.Combine(root, relativePath));
            if (!candidate.StartsWith(root, StringComparison.Ordinal))
            {
                continue;
            }

            if (!File.Exists(candidate))
            {
                continue;
            }

            return new FileInfo(candidate).Length;
        }

        return 0;
    }

    private static string NormalizeRelativePath(string rawUrl)
    {
        var normalized = rawUrl.Trim();
        if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
        {
            normalized = uri.AbsolutePath;
        }

        normalized = normalized.Split('?', '#')[0];
        normalized = normalized.Replace('\\', '/');

        return normalized.TrimStart('/');
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
    public long QuotaBytes { get; init; } = ProfileEndpoints.StorageQuotaInBytes;
    public long RemainingBytes { get; init; } = ProfileEndpoints.StorageQuotaInBytes;
}
