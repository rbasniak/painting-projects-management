using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Materials;
public class MaterialDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public EnumReference UnitPriceUnit { get; set; }
    public double UnitPriceAmount { get; set; }

    public static MaterialDetails FromModel(Material material)
    {
        return new MaterialDetails
        {
            Id = material.Id,
            Name = material.Name,
            UnitPriceUnit = new(material.UnitPriceUnit),
            UnitPriceAmount = material.UnitPriceAmount
        };
    }
}
