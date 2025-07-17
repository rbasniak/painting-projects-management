namespace PaintingProjectsManagement.Features.Materials;

public class Material
{
    // Constructor for EF Core, do not remote it or make it public
    private Material() { }

    public Material(string name, MaterialUnit unit, double pricePerUnit)
    {
        Name = name;
        Unit = unit;
        PricePerUnit = pricePerUnit;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public MaterialUnit Unit { get; private set; }
    public double PricePerUnit { get; private set; }

    public void UpdateDetails(string name, MaterialUnit unit, double pricePerUnit)
    {
        Name = name;
        Unit = unit;
        PricePerUnit = pricePerUnit;
    } 
}
