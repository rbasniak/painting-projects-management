namespace PaintingProjectsManagement.Blazor.Modules.Authentication;

public sealed record StorageUsageResponse
{
    public long UsedBytes { get; init; }
    public long QuotaBytes { get; init; } = 100L * 1024 * 1024;
    public long RemainingBytes { get; init; } = 100L * 1024 * 1024;
}
