namespace PaintingProjectsManagement.UI.Modules.Inventory;

public interface IInventoryService
{
    Task<GetCatalogResponse> GetCatalogAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MyPaintDetails>> GetMyPaintsAsync(CancellationToken cancellationToken);

    Task AddToMyPaintsAsync(IReadOnlyList<Guid> paintColorIds, CancellationToken cancellationToken);

    Task RemoveFromMyPaintsAsync(Guid paintColorId, CancellationToken cancellationToken);
}
