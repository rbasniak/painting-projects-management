namespace PaintingProjectsManagement.Features.Paints;

public class PaintLine
{
    // Constructor for EF Core, do not remote it or make it public
    private PaintLine() { }

    public PaintLine(PaintBrand brand, Guid id, string name)
    {
        Id = id;
        Name = name;
        BrandId = brand.Id;
        Brand = brand;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid BrandId { get; private set; }
    public PaintBrand Brand { get; private set; }

    public void UpdateDetails(string name, PaintBrand brand)
    {
        Name = name;
        BrandId = brand.Id;
        Brand = brand;
    }
}