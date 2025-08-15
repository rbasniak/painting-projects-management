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
        HttpResponseMessage response = await this._httpClient.GetAsync("api/materials", cancellationToken);
        response.EnsureSuccessStatusCode();
        IReadOnlyCollection<MaterialDetails> allAsync = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MaterialDetails>>();
        response = (HttpResponseMessage)null;
        return allAsync;
    }

    public async Task<MaterialDetails> CreateAsync(
      CreateMaterialRequest request,
      CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await this._httpClient.PostAsJsonAsync<CreateMaterialRequest>("api/materials", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        MaterialDetails async = await response.Content.ReadFromJsonAsync<MaterialDetails>();
        response = (HttpResponseMessage)null;
        return async;
    }

    public async Task<MaterialDetails> UpdateAsync(
      UpdateMaterialRequest request,
      CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await this._httpClient.PutAsJsonAsync<UpdateMaterialRequest>("api/materials", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        MaterialDetails materialDetails = await response.Content.ReadFromJsonAsync<MaterialDetails>();
        response = (HttpResponseMessage)null;
        return materialDetails;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await this._httpClient.DeleteAsync($"api/materials/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
        response = (HttpResponseMessage)null;
    }
}
