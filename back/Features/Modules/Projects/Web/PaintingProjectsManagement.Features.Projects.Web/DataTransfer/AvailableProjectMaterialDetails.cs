namespace PaintingProjectsManagement.Features.Projects;

public class AvailableProjectMaterialDetails
{
    public required Guid MaterialId { get; init; }
    public required string MaterialName { get; init; } = string.Empty;
    public required int CategoryId { get; init; }
    public required string CategoryName { get; init; } = string.Empty;
    public required MaterialUnit DefaultUnit { get; init; }
}
