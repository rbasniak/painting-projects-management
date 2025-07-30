namespace PaintingProjectsManagement.Features.Projects;

public class MaterialForProject
{
    private MaterialForProject()
    {
        // EF Core constructor, don't remove it
    }

    public MaterialForProject(Guid projectId, Guid materialId, double quantity)
    {
        MaterialId = materialId;
        ProjectId = projectId;
        Quantity = quantity;
    }

    public Guid MaterialId { get; private set; }
    public Guid ProjectId { get; private set; }
    public double Quantity { get; private set; } = 0.0;
}