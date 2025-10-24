namespace PaintingProjectsManagement.Features.Models;

public class ModelCategory: TenantEntity
{
    // Constructor for EF Core, do not remote it or make it public
    private ModelCategory() { }

    public ModelCategory(string tenant, string name)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        TenantId = tenant;
    }

    public string Name { get; private set; } = string.Empty;

    public void UpdateDetails(string name)
    {
        Name = name;
    } 
}
