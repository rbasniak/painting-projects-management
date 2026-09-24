namespace PaintingProjectsManagement.Features.Authorization;

public sealed record StorageUsageDetails
{
    public long UsedBytes { get; init; }
    public long QuotaBytes { get; init; }
    public long RemainingBytes { get; init; }
}
