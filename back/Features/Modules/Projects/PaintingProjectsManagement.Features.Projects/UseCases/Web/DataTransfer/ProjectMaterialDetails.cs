using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectMaterialDetails
{
    public required Guid MaterialId { get; init; }
    public required string MaterialName { get; init; } = string.Empty;
    public required int CategoryId { get; init; }  
    public required string CategoryName { get; init; } = string.Empty;
    public required double Quantity { get; init; }
    public required string QuantityFormatted { get; init; } = string.Empty;
    public required MaterialUnit Unit { get; init; }
}