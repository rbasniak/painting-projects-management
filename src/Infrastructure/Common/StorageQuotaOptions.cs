namespace PaintingProjectsManagement.Infrastructure.Common;

public sealed class StorageQuotaOptions
{
    public const string SectionName = "StorageQuota";
    public const long DefaultQuotaInBytes = 100L * 1024 * 1024;

    public long QuotaInBytes { get; set; } = DefaultQuotaInBytes;
}
