namespace PaintingProjectsManagement.Features.Paints;

public class PaintBrandDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static PaintBrandDetails FromModel(PaintBrand brand)
    {
        return new PaintBrandDetails
        {
            Id = brand.Id,
            Name = brand.Name
        };
    }
}