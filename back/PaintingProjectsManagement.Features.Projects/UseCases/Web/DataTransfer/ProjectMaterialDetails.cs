using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectMaterialDetails
{
    public Guid MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string PricePerUnitFormatted { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string QuantityFormatted { get; set; } = string.Empty;
    public MaterialUnit Unit { get; set; }
    public string UnitDisplayName { get; set; } = string.Empty;
}