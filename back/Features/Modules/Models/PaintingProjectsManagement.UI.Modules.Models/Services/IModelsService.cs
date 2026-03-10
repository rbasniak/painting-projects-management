using System.Net.Http.Json;

namespace PaintingProjectsManagement.UI.Modules.Models;

public interface IModelsService
{
    Task<IReadOnlyCollection<ModelDetails>> GetAllAsync(CancellationToken cancellationToken);
    Task<ModelDetails> CreateAsync(CreateModelRequest request, CancellationToken cancellationToken);
    Task<ModelDetails> UpdateAsync(UpdateModelRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task SetMustHaveAsync(Guid id, bool mustHave, CancellationToken cancellationToken);
    Task RateModelAsync(Guid id, int score, CancellationToken cancellationToken);
    Task SetBaseSizeAsync(Guid id, BaseSize baseSize, CancellationToken cancellationToken);
    Task SetFigureSizeAsync(Guid id, FigureSize figureSize, CancellationToken cancellationToken);
    Task SetFigureCountAsync(Guid id, int numberOfFigures, CancellationToken cancellationToken);
    Task<ModelDetails> UploadPictureAsync(UploadModelPictureRequest request, CancellationToken cancellationToken);
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

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ModelDetails>>();
            return result ?? Array.Empty<ModelDetails>();
        }

        return Array.Empty<ModelDetails>();
    }

    public async Task<ModelDetails> CreateAsync(CreateModelRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/models", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ModelDetails>();
            return result ?? new ModelDetails();
        }

        return new ModelDetails();
    }

    public async Task<ModelDetails> UpdateAsync(UpdateModelRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync("api/models", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ModelDetails>();
            return result ?? new ModelDetails();
        }

        return new ModelDetails();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _httpClient.DeleteAsync($"api/models/{id}", cancellationToken);
    }

    public async Task SetMustHaveAsync(Guid id, bool mustHave, CancellationToken cancellationToken)
    {
        var request = new { Id = id, MustHave = mustHave };
        await _httpClient.PutAsJsonAsync("api/models/must-have", request, cancellationToken);
    }

    public async Task RateModelAsync(Guid id, int score, CancellationToken cancellationToken)
    {
        var request = new { Id = id, Score = score };
        await _httpClient.PostAsJsonAsync("api/models/rate", request, cancellationToken);
    }

    public async Task SetBaseSizeAsync(Guid id, BaseSize baseSize, CancellationToken cancellationToken)
    {
        var request = new { Id = id, BaseSize = baseSize };
        await _httpClient.PutAsJsonAsync("api/models/base-size", request, cancellationToken);
    }

    public async Task SetFigureSizeAsync(Guid id, FigureSize figureSize, CancellationToken cancellationToken)
    {
        var request = new { Id = id, FigureSize = figureSize };
        await _httpClient.PutAsJsonAsync("api/models/figure-size", request, cancellationToken);
    }

    public async Task SetFigureCountAsync(Guid id, int numberOfFigures, CancellationToken cancellationToken)
    {
        var request = new { Id = id, NumberOfFigures = numberOfFigures };
        await _httpClient.PutAsJsonAsync("api/models/figure-count", request, cancellationToken);
    }

    public async Task<ModelDetails> UploadPictureAsync(UploadModelPictureRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/models/picture", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ModelDetails>();
            return result ?? new ModelDetails();
        }

        return new ModelDetails();
    }

    public async Task PromotePictureToCoverAsync(PromoteModelPictureRequest request, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/models/picture/promote", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeletePictureAsync(Guid modelId, string pictureUrl, CancellationToken cancellationToken)
    {
        var request = new DeleteModelPictureRequest
        {
            ModelId = modelId,
            PictureUrl = pictureUrl
        };

        using var response = await _httpClient.PostAsJsonAsync("api/models/picture/delete", request, cancellationToken);
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

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ModelCategoryDetails>>();
            return result ?? Array.Empty<ModelCategoryDetails>();
        }

        return Array.Empty<ModelCategoryDetails>();
    }

    public async Task<ModelCategoryDetails> CreateAsync(CreateModelCategoryRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/models/categories", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ModelCategoryDetails>();
            return result ?? new ModelCategoryDetails();
        }

        return new ModelCategoryDetails();
    }

    public async Task<ModelCategoryDetails> UpdateAsync(UpdateModelCategoryRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync("api/models/categories", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ModelCategoryDetails>();
            return result ?? new ModelCategoryDetails();
        }

        return new ModelCategoryDetails();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _httpClient.DeleteAsync($"api/models/categories/{id}", cancellationToken);
    }
}
