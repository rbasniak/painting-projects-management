using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace PaintingProjectsManagement.Infrastructure.Common;

public interface ITenantStorageUsageService
{
    long QuotaInBytes { get; }
    long GetImageBytes(string base64Image);
    long GetFileSizeInBytes(string? rawUrl);
    Task<long> GetUsageInBytesAsync(string tenant, CancellationToken cancellationToken);
    Task<bool> HasQuotaForImageAsync(string tenant, string base64Image, long bytesToRelease, CancellationToken cancellationToken);
}

public sealed class TenantStorageUsageService(
    IWebHostEnvironment environment,
    IOptions<StorageQuotaOptions> options) : ITenantStorageUsageService
{
    public long QuotaInBytes => options.Value.QuotaInBytes <= 0
        ? StorageQuotaOptions.DefaultQuotaInBytes
        : options.Value.QuotaInBytes;

    public async Task<bool> HasQuotaForImageAsync(string tenant, string base64Image, long bytesToRelease, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenant))
        {
            return false;
        }

        var incomingBytes = GetImageBytes(base64Image);
        var usedBytes = await GetUsageInBytesAsync(tenant, cancellationToken);
        var effectiveUsage = Math.Max(0, usedBytes - Math.Max(0, bytesToRelease));

        return effectiveUsage + incomingBytes <= QuotaInBytes;
    }

    public long GetImageBytes(string base64Image)
    {
        if (string.IsNullOrWhiteSpace(base64Image))
        {
            return 0;
        }

        var base64Payload = base64Image;
        var commaIndex = base64Image.IndexOf(',');
        if (commaIndex >= 0 && commaIndex < base64Image.Length - 1)
        {
            base64Payload = base64Image[(commaIndex + 1)..];
        }

        var bytes = Convert.FromBase64String(base64Payload);
        return bytes.LongLength;
    }

    public Task<long> GetUsageInBytesAsync(string tenant, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenant))
        {
            return Task.FromResult(0L);
        }

        var total = 0L;
        foreach (var uploadsRoot in GetUploadsRoots())
        {
            if (!Directory.Exists(uploadsRoot))
            {
                continue;
            }

            var tenantDirectories = Directory.EnumerateDirectories(uploadsRoot)
                .Where(path => string.Equals(new DirectoryInfo(path).Name, tenant, StringComparison.OrdinalIgnoreCase));

            foreach (var tenantDirectory in tenantDirectories)
            {
                total += Directory.EnumerateFiles(tenantDirectory, "*", SearchOption.AllDirectories)
                    .Select(path => new FileInfo(path).Length)
                    .Sum();
            }
        }

        return Task.FromResult(total);
    }

    public long GetFileSizeInBytes(string? rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            return 0;
        }

        var relativePath = NormalizeRelativePath(rawUrl);
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return 0;
        }

        foreach (var webRoot in GetWebRoots())
        {
            var candidate = Path.GetFullPath(Path.Combine(webRoot, relativePath));
            if (!candidate.StartsWith(webRoot, StringComparison.Ordinal))
            {
                continue;
            }

            if (File.Exists(candidate))
            {
                return new FileInfo(candidate).Length;
            }
        }

        return 0;
    }

    private IEnumerable<string> GetUploadsRoots()
    {
        return GetWebRoots()
            .Select(path => Path.Combine(path, "uploads"))
            .Distinct(StringComparer.Ordinal);
    }

    private IEnumerable<string> GetWebRoots()
    {
        var roots = new[]
        {
            environment.WebRootPath,
            Path.Combine(environment.ContentRootPath, "wwwroot"),
            Path.Combine(AppContext.BaseDirectory, "wwwroot")
        };

        return roots
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.Ordinal);
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
