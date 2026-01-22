namespace PaintingProjectsManagement.UI.Modules.Inventory;

public class PaintColorDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
    public string? ManufacturerCode { get; set; }
    public double BottleSize { get; set; }
    public int Type { get; set; }
}
