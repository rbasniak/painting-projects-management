namespace PaintingProjectsManagement.Features.Projects;

public class ReadOnlyMaterial
{
    public string Tenant { get; set; } = default!;
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public double PricePerUnit { get; set; }
    public string Unit { get; set; } = default!;
    public DateTime UpdatedUtc { get; set; }
}
