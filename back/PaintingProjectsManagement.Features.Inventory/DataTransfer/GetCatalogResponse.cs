namespace PaintingProjectsManagement.Features.Inventory;

public class GetCatalogResponse
{
    public IReadOnlyList<PaintBrandDetails> Brands { get; init; } = Array.Empty<PaintBrandDetails>();
}
