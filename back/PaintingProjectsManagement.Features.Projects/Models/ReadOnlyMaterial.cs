namespace PaintingProjectsManagement.Features.Projects;

public sealed record ReadOnlyMaterial
{
    public required string Tenant { get; init; } = default!;
    public required Guid Id { get; init; }
    public required string Name { get; set; } = default!;
    public required double PricePerUnit { get; set; }
    public required string Unit { get; set; } = default!;
    public required DateTime UpdatedUtc { get; set; }
}
