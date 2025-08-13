using System;

namespace PaintingProjectsManagement.Features.Projects;

/// <summary>
/// Local read model representing a material as received from integration events.
/// </summary>
public class MaterialLocalCopy
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public double PricePerUnit { get; set; }
    public string Unit { get; set; } = default!;
    public DateTime UpdatedUtc { get; set; }
}
