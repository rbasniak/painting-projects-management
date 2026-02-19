namespace PaintingProjectsManagement.Features.Inventory.Web;

public interface ICatalogDetails
{
    IReadOnlyList<IPaintBrandDetails> Brands { get; }
}
