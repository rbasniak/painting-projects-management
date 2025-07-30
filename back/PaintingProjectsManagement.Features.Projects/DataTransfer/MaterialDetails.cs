using PaintingProjectsManagement.Features.Materials;
using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MaterialUnit Unit { get; set; }
    public double PricePerUnit { get; set; }
    public double Quantity { get; set; } = 0.0;

    public static MaterialDetails FromModel(ReadOnlyMaterial material, MaterialForProject projectMaterial)
    {
        if (projectMaterial.MaterialId != material.Id)
        {
            throw new ArgumentException("Material ID does not match the project material ID.", nameof(projectMaterial));
        }

        return new MaterialDetails
        {
            Id = material.Id,
            Name = material.Name,
            Unit = material.Unit,
            Quantity = projectMaterial.Quantity,
            PricePerUnit = material.PricePerUnit
        };
    }
}
