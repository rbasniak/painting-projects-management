using System;
using System.Net.Http;
using System.Net.Http.Json;

namespace PaintingProjectsManagement.UI.Modules.Models;

public interface IModelsService
{
    Task<IReadOnlyCollection<ModelDetails>> GetAllAsync(CancellationToken cancellationToken);
    Task<ModelDetails> CreateAsync(CreateModelRequest request, CancellationToken cancellationToken);
    Task<ModelDetails> UpdateAsync(UpdateModelRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task SetMustHaveAsync(Guid id, bool mustHave, CancellationToken cancellationToken);
    Task<string> UploadPictureAsync(UploadModelPictureRequest request, CancellationToken cancellationToken);
    Task PromotePictureToCoverAsync(PromoteModelPictureRequest request, CancellationToken cancellationToken);
    Task DeletePictureAsync(Guid modelId, string pictureUrl, CancellationToken cancellationToken);
}

public interface IModelCategoriesService
{
    Task<IReadOnlyCollection<ModelCategoryDetails>> GetAllAsync(CancellationToken cancellationToken);
    Task<ModelCategoryDetails> CreateAsync(CreateModelCategoryRequest request, CancellationToken cancellationToken);
    Task<ModelCategoryDetails> UpdateAsync(UpdateModelCategoryRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public class ModelsService : IModelsService
{
    private readonly HttpClient _httpClient;

    public ModelsService(HttpClient httpClient) => this._httpClient = httpClient;

    public async Task<IReadOnlyCollection<ModelDetails>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/models", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ModelDetails>>();
        return result ?? [];
    }

    public async Task<ModelDetails> CreateAsync(CreateModelRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/models", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ModelDetails>();
        return result ?? new ModelDetails();
    }

    public async Task<ModelDetails> UpdateAsync(UpdateModelRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync("api/models", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ModelDetails>();
        return result ?? new ModelDetails();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/models/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetMustHaveAsync(Guid id, bool mustHave, CancellationToken cancellationToken)
    {
        var request = new { MustHave = mustHave };
        var response = await _httpClient.PutAsJsonAsync($"api/models/{id}/must-have", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> UploadPictureAsync(UploadModelPictureRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/models/picture", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<string>();
        return result ?? string.Empty;
    }

    public async Task PromotePictureToCoverAsync(PromoteModelPictureRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/models/{request.ModelId}/promote-picture", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeletePictureAsync(Guid modelId, string pictureUrl, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/models/{modelId}/picture?pictureUrl={Uri.EscapeDataString(pictureUrl)}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

public class ModelCategoriesService : IModelCategoriesService
{
    private readonly HttpClient _httpClient;

    public ModelCategoriesService(HttpClient httpClient) => this._httpClient = httpClient;

    public async Task<IReadOnlyCollection<ModelCategoryDetails>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/models/categories", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ModelCategoryDetails>>();
        return result ?? [];
    }

    public async Task<ModelCategoryDetails> CreateAsync(CreateModelCategoryRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/models/categories", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ModelCategoryDetails>();
        return result ?? new ModelCategoryDetails();
    }

    public async Task<ModelCategoryDetails> UpdateAsync(UpdateModelCategoryRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync("api/models/categories", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ModelCategoryDetails>();
        return result ?? new ModelCategoryDetails();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/models/categories/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
