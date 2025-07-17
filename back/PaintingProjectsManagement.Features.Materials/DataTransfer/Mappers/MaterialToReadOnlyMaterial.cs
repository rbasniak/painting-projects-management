using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Materials;

public static class MaterialToReadOnlyMaterial
{
    public static ReadOnlyMaterial MapFromModel(this Material model)
    {
        return new ReadOnlyMaterial
        {
            Id = model.Id,
            Name = model.Name,
            Unit = model.Unit,
            PricePerUnit = model.PricePerUnit
        };
    }
}
