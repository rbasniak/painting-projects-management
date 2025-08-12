namespace PaintingProjectsManagement.Features.Materials.Abstractions;
public class ReadOnlyMaterial
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    // public MaterialUnit Unit { get; init; }
    // public double PricePerUnit { get; init; }
}