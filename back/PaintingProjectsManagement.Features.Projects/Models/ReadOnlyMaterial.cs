namespace PaintingProjectsManagement.Features.Projects;

public sealed record ReadOnlyMaterial
{
    public required string Tenant { get; init; } = default!;
    public required Guid Id { get; init; }
    public required string Name { get; init; } = default!;
    public required double PricePerUnit { get; init; }
    public required string Unit { get; init; } = default!;
    public required DateTime UpdatedUtc { get; init; }
}
