
using PaintingProjectsManagement.Features.Inventory.Web;
using PaintingProjectsManagement.Infrastructure.Common;

namespace PaintingProjectsManagement.Features.Inventory;

public class PaintColorDetails : IPaintColorDetails
{
    public required Guid Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required string HexColor { get; init; } = string.Empty;
    public required string? ManufacturerCode { get; init; }
    public required double BottleSize { get; init; } 
    public required PaintType Type { get; init; }
    public required EntityReference Line { get; init; }
    public required EntityReference Brand { get; init; }

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
}