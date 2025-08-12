using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Materials;

public static class MaterialToReadOnlyMaterial
{
    public static ReadOnlyMaterial MapFromModel(this Material model)
    {
        // Temporarily adapt to ReadOnlyMaterial until it is reworked
        return new ReadOnlyMaterial
        {
            Id = model.Id,
            Name = model.Name,
            // Unit = model.UnitPriceUnit, // to be reworked with ReadOnlyMaterial
            // PricePerUnit = model.UnitPriceAmount
        };
    }
}
