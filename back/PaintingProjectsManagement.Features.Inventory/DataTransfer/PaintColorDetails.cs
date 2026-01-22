using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Inventory;

public class PaintColorDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
    public string? ManufacturerCode { get; set; }
    public double BottleSize { get; set; } 
    public PaintType Type { get; set; }
    public EntityReference Line { get; set; }
    public EntityReference Brand { get; set; }

    public static PaintColorDetails FromModel(PaintColor paint)
    {
        return new PaintColorDetails
        {
            Id = paint.Id,
            Name = paint.Name,
            HexColor = paint.HexColor,
            ManufacturerCode = paint.ManufacturerCode,
            BottleSize = paint.BottleSize,
            Type = paint.Type,
            Line = new EntityReference(paint.LineId, paint.Line.Name),
            Brand = new EntityReference(paint.Line.Brand.Id, paint.Line.Brand.Name)
        };
    }

    public static PaintColorDetails FromModelForCatalog(PaintColor paint)
    {
        return new PaintColorDetails
        {
            Id = paint.Id,
            Name = paint.Name,
            HexColor = paint.HexColor,
            ManufacturerCode = paint.ManufacturerCode,
            BottleSize = paint.BottleSize,
            Type = paint.Type,
            Line = new EntityReference(paint.LineId, paint.Line.Name),
            Brand = new EntityReference(paint.Line.Brand.Id, paint.Line.Brand.Name)
        };
    }
}