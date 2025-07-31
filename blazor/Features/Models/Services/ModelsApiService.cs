using System.Net.Http.Json;
using PaintingProjectsManagement.Blazor.Features.Models.Models;

namespace PaintingProjectsManagement.Blazor.Features.Models.Services;

public class ModelsApiService : IModelsApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://localhost:7236";

    public ModelsApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_baseUrl);
    }

    public async Task<List<Model>> GetModelsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Model>>("/api/models");
            return response ?? new List<Model>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching models: {ex.Message}");
            return new List<Model>();
        }
    }

    public async Task<Model?> GetModelAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Model>($"/api/models/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching model {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<Model> CreateModelAsync(CreateModelRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/models", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Model>() ?? new Model();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating model: {ex.Message}");
            throw;
        }
    }

    public async Task<Model> UpdateModelAsync(Guid id, UpdateModelRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/models/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Model>() ?? new Model();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating model {id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteModelAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/models/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting model {id}: {ex.Message}");
            throw;
        }
    }

    public async Task RateModelAsync(Guid id, int score)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/models/{id}/rate", new { score });
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rating model {id}: {ex.Message}");
            throw;
        }
    }

    public async Task SetMustHaveAsync(Guid id, bool mustHave)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/models/{id}/must-have", new { mustHave });
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting must-have for model {id}: {ex.Message}");
            throw;
        }
    }
} 