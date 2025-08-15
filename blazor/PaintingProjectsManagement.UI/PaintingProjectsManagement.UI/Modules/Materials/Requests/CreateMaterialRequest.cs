namespace PaintingProjectsManagement.UI.Modules.Materials;

public class CreateMaterialRequest
{
    public string Name { get; set; } = string.Empty;

    public MaterialUnit Unit { get; set; }

    public double PricePerUnit { get; set; }
}