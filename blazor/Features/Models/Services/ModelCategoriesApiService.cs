using System.Net.Http.Json;
using PaintingProjectsManagement.Blazor.Features.Models.Models;

namespace PaintingProjectsManagement.Blazor.Features.Models.Services;

public class ModelCategoriesApiService : IModelCategoriesApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://localhost:7236";

    public ModelCategoriesApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_baseUrl);
    }

    public async Task<List<ModelCategory>> GetModelCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<ModelCategory>>("/api/models/categories");
            return response ?? new List<ModelCategory>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching model categories: {ex.Message}");
            return new List<ModelCategory>();
        }
    }

    public async Task<ModelCategory?> GetModelCategoryAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ModelCategory>($"/api/models/categories/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching model category {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<ModelCategory> CreateModelCategoryAsync(CreateModelCategoryRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/models/categories", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ModelCategory>() ?? new ModelCategory();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating model category: {ex.Message}");
            throw;
        }
    }

    public async Task<ModelCategory> UpdateModelCategoryAsync(Guid id, UpdateModelCategoryRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/models/categories/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ModelCategory>() ?? new ModelCategory();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating model category {id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteModelCategoryAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/models/categories/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting model category {id}: {ex.Message}");
            throw;
        }
    }
} 