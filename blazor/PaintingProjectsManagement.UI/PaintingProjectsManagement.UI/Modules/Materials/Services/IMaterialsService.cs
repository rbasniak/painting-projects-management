using System.Net.Http.Json;

namespace PaintingProjectsManagement.UI.Modules.Materials;

public interface IMaterialsService
{
    Task<IReadOnlyCollection<MaterialDetails>> GetAllAsync(CancellationToken cancellationToken);

    Task<MaterialDetails> CreateAsync(
      CreateMaterialRequest request,
      CancellationToken cancellationToken);

    Task<MaterialDetails> UpdateAsync(
      UpdateMaterialRequest request,
      CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public class MaterialsService : IMaterialsService
{
    private readonly HttpClient _httpClient;

    public MaterialsService(HttpClient httpClient) => this._httpClient = httpClient;

    public async Task<IReadOnlyCollection<MaterialDetails>> GetAllAsync(
      CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/materials", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MaterialDetails>>();
        return result;
    }

    public async Task<MaterialDetails> CreateAsync(
      CreateMaterialRequest request,
      CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/materials", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MaterialDetails>();

        return result;
    }

    public async Task<MaterialDetails> UpdateAsync(
      UpdateMaterialRequest request,
      CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync("api/materials", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MaterialDetails>();
        return result;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/materials/{id}", cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
