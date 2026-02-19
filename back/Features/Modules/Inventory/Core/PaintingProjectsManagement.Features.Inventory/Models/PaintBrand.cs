namespace PaintingProjectsManagement.Features.Inventory;

public class PaintBrand : BaseEntity
{
    // Constructor for EF Core, do not remote it or make it public
    private PaintBrand() { }

    public PaintBrand(string name)
    {
        Name = name;
    }

    public string Name { get; set; } = string.Empty;

    public void UpdateDetails(string name)
    {
        Name = name;
    }
}