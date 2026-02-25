namespace PaintingProjectsManagement.Features.Inventory.Web;

public interface IMyPaintDetails
{
    Guid PaintColorId { get; }    
    string Name { get; }
    string HexColor { get; }
    string BrandName { get; }
    string LineName { get; }
}