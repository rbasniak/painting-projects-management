namespace PaintingProjectsManagement.Features.Paints;

public class PaintBrand
{
    // Constructor for EF Core, do not remote it or make it public
    private PaintBrand() { }

    public PaintBrand(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public void UpdateDetails(string name)
    {
        Name = name;
    }
}