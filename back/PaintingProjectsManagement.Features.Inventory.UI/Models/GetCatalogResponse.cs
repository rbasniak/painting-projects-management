namespace PaintingProjectsManagement.UI.Modules.Inventory;

public class GetCatalogResponse
{
    public IReadOnlyList<PaintBrandDetails> Brands { get; set; } = Array.Empty<PaintBrandDetails>();
}
