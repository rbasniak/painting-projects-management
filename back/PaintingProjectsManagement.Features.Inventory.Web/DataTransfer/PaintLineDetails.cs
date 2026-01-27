using PaintingProjectsManagement.Features.Inventory.Web;
using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Inventory;

public class PaintLineDetails  
{
    public required Guid Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required EntityReference Brand { get; init; }
    public IReadOnlyList<PaintColorDetails> Paints { get; init; } = Array.Empty<PaintColorDetails>();

    public static PaintLineDetails FromModel(PaintLine line)
    {
        return new PaintLineDetails
        {
            Id = line.Id,
            Name = line.Name,
            Brand = new EntityReference(line.BrandId, line.Brand.Name)
        };
    }
}