using PaintingProjectsManagement.UI.Client.Models;

namespace PaintingProjectsManagement.UI.Client.Modules.Materials.Models;

public class CreateMaterialRequest
{
    public string Name { get; set; } = string.Empty;
    public MaterialUnit Unit { get; set; }
    public double PricePerUnit { get; set; }
}

 