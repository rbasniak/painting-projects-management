namespace PaintingProjectsManagement.UI.Modules.Projects;

public class ProjectMaterialDetails
{
    public Guid MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string PricePerUnit { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public int Unit { get; set; }
}
