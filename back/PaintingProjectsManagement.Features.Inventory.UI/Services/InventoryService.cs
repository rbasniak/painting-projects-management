using System.Net.Http.Json;

namespace PaintingProjectsManagement.UI.Modules.Inventory;

public class InventoryService : IInventoryService
{
    private readonly HttpClient _httpClient;

    public InventoryService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<GetCatalogResponse> GetCatalogAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/inventory/catalog", cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GetCatalogResponse>(cancellationToken);
        return result ?? new GetCatalogResponse();
    }

    public async Task<IReadOnlyCollection<MyPaintDetails>> GetMyPaintsAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/inventory/my-paints", cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MyPaintDetails>>(cancellationToken);
        return result ?? Array.Empty<MyPaintDetails>();
    }

    public async Task AddToMyPaintsAsync(IReadOnlyList<Guid> paintColorIds, CancellationToken cancellationToken)
    {
        var request = new AddToMyPaintsRequest { PaintColorIds = paintColorIds };
        var response = await _httpClient.PostAsJsonAsync("api/inventory/my-paints", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveFromMyPaintsAsync(Guid paintColorId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/inventory/my-paints/{paintColorId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
