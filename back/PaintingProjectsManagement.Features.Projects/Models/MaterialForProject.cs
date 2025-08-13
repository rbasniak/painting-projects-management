using PaintingProjectsManagement.Features.Materials;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialForProject
{
    public MaterialForProject(Guid projectId, Guid materialId, double quantity, PackageUnit unit)
    {
        MaterialId = materialId;
        ProjectId = projectId;
        Quantity = quantity;
        Unit = unit;
    }

    public Guid MaterialId { get; private set; }
    public Guid ProjectId { get; private set; }
    public double Quantity { get; private set; }
    public PackageUnit Unit { get; private set; }
}