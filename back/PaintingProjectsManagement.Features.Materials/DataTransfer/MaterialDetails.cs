using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Materials;
public class MaterialDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public EnumReference Unit { get; set; }
    public double PricePerUnit { get; set; }

    public static MaterialDetails FromModel(Material material)
    {
        return new MaterialDetails
        {
            Id = material.Id,
            Name = material.Name,
            Unit = new (material.Unit),
            PricePerUnit = material.PricePerUnit
        };
    }
}
