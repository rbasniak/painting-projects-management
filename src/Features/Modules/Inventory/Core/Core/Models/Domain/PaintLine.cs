namespace PaintingProjectsManagement.Features.Inventory;

public class PaintLine : BaseEntity
{
    // Constructor for EF Core, do not remote it or make it public
    private PaintLine() { }

    public PaintLine(PaintBrand brand, string name)
    {
        Name = name;
        Brand = brand;
    }

    public string Name { get; private set; } = string.Empty;
    public Guid BrandId { get; private set; }
    public PaintBrand Brand { get; private set; }

    public void UpdateDetails(string name)
    {
        Name = name;
    }
}