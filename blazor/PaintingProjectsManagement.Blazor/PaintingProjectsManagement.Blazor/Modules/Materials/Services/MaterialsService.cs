using System.Net.Http.Json;

namespace PaintingProjectsManagement.Blazor.Modules.Materials;

public interface IMaterialsService
{
    Task<IReadOnlyCollection<MaterialDetails>> GetMaterialsAsync(CancellationToken cancellationToken);
    Task<MaterialDetails> CreateMaterialAsync(CreateMaterialRequest request, CancellationToken cancellationToken);
    Task<MaterialDetails> UpdateMaterialAsync(UpdateMaterialRequest request, CancellationToken cancellationToken);
    Task DeleteMaterialAsync(Guid id, CancellationToken cancellationToken);
}

public class MaterialsService : IMaterialsService
{
    private readonly HttpClient _httpClient;
    
    public MaterialsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<MaterialDetails>> GetMaterialsAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/materials", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MaterialDetails>>();
    }

    public async Task<MaterialDetails> CreateMaterialAsync(CreateMaterialRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/materials", request, cancellationToken);

        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<MaterialDetails>();
    }

    public async Task<MaterialDetails> UpdateMaterialAsync(UpdateMaterialRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/materials", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MaterialDetails>();
    }

    public async Task DeleteMaterialAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/materials/{id}", cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
