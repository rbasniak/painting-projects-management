namespace PaintingProjectsManagement.Features.Paints;

public class PaintColor : BaseEntity
{
    // Constructor for EF Core, do not remote it or make it public
    private PaintColor() { }

    public PaintColor(PaintLine line, string name, string hexColor, double bottleSize, PaintType type, string? manufacturerCode = null)
    {
        Name = name;
        HexColor = hexColor;
        BottleSize = bottleSize;
        Type = type;
        Line = line;
        LineId = line.Id;
        ManufacturerCode = manufacturerCode;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string HexColor { get; private set; } = string.Empty;
    public string? ManufacturerCode { get; private set; }
    public double BottleSize { get; private set; } 
    public PaintType Type { get; private set; }
    public Guid LineId { get; private set; }
    public PaintLine Line { get; private set; }

    public void UpdateDetails(string name, string hexColor, double bottleSize, PaintType type, PaintLine line, string? manufacturerCode = null)
    {
        Name = name;
        HexColor = hexColor;
        BottleSize = bottleSize; 
        Type = type;
        ManufacturerCode = manufacturerCode;
        Line = line;
        LineId = line.Id;
    } 
}