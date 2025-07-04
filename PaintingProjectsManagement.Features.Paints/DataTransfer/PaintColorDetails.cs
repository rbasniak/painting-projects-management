namespace PaintingProjectsManagement.Features.Paints;

public class PaintColorDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
    public string? ManufacturerCode { get; set; }
    public double BottleSize { get; set; }
    public double Price { get; set; }
    public PaintType Type { get; set; }
    public Guid LineId { get; set; }
    public string? LineName { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }

    public static PaintColorDetails FromModel(PaintColor paint)
    {
        return new PaintColorDetails
        {
            Id = paint.Id,
            Name = paint.Name,
            HexColor = paint.HexColor,
            ManufacturerCode = paint.ManufacturerCode,
            BottleSize = paint.BottleSize,
            Price = paint.Price,
            Type = paint.Type,
            LineId = paint.LineId,
            LineName = paint.Line?.Name,
            BrandId = paint.Line?.Brand?.Id,
            BrandName = paint.Line?.Brand?.Name
        };
    }
}