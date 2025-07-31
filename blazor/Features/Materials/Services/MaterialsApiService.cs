using System.Net.Http.Json;
using PaintingProjectsManagement.Blazor.Features.Materials.Models;
using PaintingProjectsManagement.Blazor.Shared.Configuration;

namespace PaintingProjectsManagement.Blazor.Features.Materials.Services;

public class MaterialsApiService : IMaterialsApiService
{
    private readonly HttpClient _httpClient;

    public MaterialsApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(ApiConfiguration.BaseUrl);
    }

    public async Task<List<Material>> GetMaterialsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Material>>(ApiConfiguration.Endpoints.Materials);
            return response ?? new List<Material>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching materials: {ex.Message}");
            return new List<Material>();
        }
    }

    public async Task<Material?> GetMaterialAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Material>($"{ApiConfiguration.Endpoints.Materials}/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching material {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<Material> CreateMaterialAsync(CreateMaterialRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(ApiConfiguration.Endpoints.Materials, request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Material>() ?? new Material();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating material: {ex.Message}");
            throw;
        }
    }

    public async Task<Material> UpdateMaterialAsync(Guid id, UpdateMaterialRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{ApiConfiguration.Endpoints.Materials}/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Material>() ?? new Material();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating material {id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteMaterialAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{ApiConfiguration.Endpoints.Materials}/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting material {id}: {ex.Message}");
            throw;
        }
    }
} 