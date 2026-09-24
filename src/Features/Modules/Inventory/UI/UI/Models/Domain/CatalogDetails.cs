using PaintingProjectsManagement.Features.Inventory.Web;
using PaintingProjectsManagement.UI.Modules.Inventory;

public class CatalogDetails : ICatalogDetails
{
    public IReadOnlyList<PaintBrandDetails> Brands { get; set; } = [];

    IReadOnlyList<IPaintBrandDetails> ICatalogDetails.Brands => Brands;
}
