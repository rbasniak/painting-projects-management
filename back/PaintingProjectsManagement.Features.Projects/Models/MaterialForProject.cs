namespace PaintingProjectsManagement.Features.Projects;

public class MaterialForProject
{
    private MaterialForProject() 
    { 
    }

    public MaterialForProject(Guid projectId, Guid materialId, double quantity, MaterialUnit unit)
    {
        MaterialId = materialId;
        ProjectId = projectId;
        Quantity = new Quantity(quantity, unit);
    }

    public Guid MaterialId { get; private set; }
    public Guid ProjectId { get; private set; }
    public Quantity Quantity { get; private set; }

    public void UpdateQuantity(double quantity, MaterialUnit unit)
    {
        Quantity = new Quantity(quantity, unit);
    }
}