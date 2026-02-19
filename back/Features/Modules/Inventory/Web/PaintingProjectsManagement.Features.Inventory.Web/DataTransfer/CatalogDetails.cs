using PaintingProjectsManagement.Features.Inventory.Web;

namespace PaintingProjectsManagement.Features.Inventory;

public record CatalogDetails : ICatalogDetails
{
    public required IReadOnlyList<PaintBrandDetails> Brands { get; init; }

    IReadOnlyList<IPaintBrandDetails> ICatalogDetails.Brands => Brands;
}