namespace PaintingProjectsManagement.Features.Inventory.Integration;

public class ColorMatchResult
{
    public Guid PaintColorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string LineName { get; set; } = string.Empty;
    public double Distance { get; set; }
}
