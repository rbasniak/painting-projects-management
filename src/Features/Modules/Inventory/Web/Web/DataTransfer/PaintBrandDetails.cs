using PaintingProjectsManagement.Features.Inventory.Web;

namespace PaintingProjectsManagement.Features.Inventory;

public class PaintBrandDetails : IPaintBrandDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<PaintLineDetails> Lines { get; set; } = Array.Empty<PaintLineDetails>();

    IReadOnlyList<IPaintLineDetails> IPaintBrandDetails.Lines => Lines;

    public static PaintBrandDetails FromModel(PaintBrand brand)
    {
        return new PaintBrandDetails
        {
            Id = brand.Id,
            Name = brand.Name
        };
    }
}