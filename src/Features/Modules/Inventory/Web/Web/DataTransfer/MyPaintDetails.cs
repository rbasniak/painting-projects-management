using PaintingProjectsManagement.Features.Inventory.Web;

namespace PaintingProjectsManagement.Features.Inventory;

public class MyPaintDetails : IMyPaintDetails
{
    public required Guid PaintColorId { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required string HexColor { get; init; } = string.Empty;
    public required string BrandName { get; init; } = string.Empty;
    public required string LineName { get; init; } = string.Empty;
}
