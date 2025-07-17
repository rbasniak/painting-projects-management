namespace PaintingProjectsManagement.Features.Materials;
public class MaterialDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public MaterialUnit Unit { get; set; }
    public double PricePerUnit { get; set; }

    public static MaterialDetails FromModel(Material material)
    {
        return new MaterialDetails
        {
            Id = material.Id,
            Name = material.Name,
            Unit = material.Unit,
            PricePerUnit = material.PricePerUnit
        };
    }
}
