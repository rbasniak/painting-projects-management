using PaintingProjectsManagement.UI.Client.Models;

namespace PaintingProjectsManagement.UI.Client.Modules.Materials.Models;

public class UpdateMaterialRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MaterialUnit Unit { get; set; }
    public double PricePerUnit { get; set; }
} 