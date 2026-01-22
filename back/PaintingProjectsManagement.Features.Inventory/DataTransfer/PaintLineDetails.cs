using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Inventory;

public class PaintLineDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EntityReference Brand { get; set; }
    public IReadOnlyList<PaintColorDetails> Paints { get; set; } = Array.Empty<PaintColorDetails>();

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