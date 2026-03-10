namespace PaintingProjectsManagement.Infrastructure.Common;

public interface ITenantStorageUsageService
{
    long QuotaInBytes { get; }
    Task<long> GetQuotaInBytesAsync(string tenant, CancellationToken cancellationToken);
    long GetImageBytes(string base64Image);
    long GetFileSizeInBytes(string? rawUrl);
    Task<long> GetUsageInBytesAsync(string tenant, CancellationToken cancellationToken);
    Task<bool> HasQuotaForImageAsync(string tenant, string base64Image, long bytesToRelease, CancellationToken cancellationToken);
}
