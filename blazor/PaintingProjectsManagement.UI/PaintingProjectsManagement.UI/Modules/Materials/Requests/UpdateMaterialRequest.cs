namespace PaintingProjectsManagement.UI.Modules.Materials;


public class UpdateMaterialRequest
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public MaterialUnit Unit { get; set; }

    public double PricePerUnit { get; set; }
}
