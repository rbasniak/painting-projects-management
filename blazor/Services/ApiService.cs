using System.Net.Http.Json;
using PaintingProjectsManagement.Blazor.Models;

namespace PaintingProjectsManagement.Blazor.Services;

public interface IApiService
{
    Task<List<Material>> GetMaterialsAsync();
    Task<List<Model>> GetModelsAsync();
    Task<List<ModelCategory>> GetModelCategoriesAsync();
    Task<List<Project>> GetProjectsAsync();
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://localhost:7236"; // API base URL

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_baseUrl);
    }

    public async Task<List<Material>> GetMaterialsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Material>>("/api/materials");
            return response ?? new List<Material>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching materials: {ex.Message}");
            return new List<Material>();
        }
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

    public async Task<List<Project>> GetProjectsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Project>>("/api/projects");
            return response ?? new List<Project>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching projects: {ex.Message}");
            return new List<Project>();
        }
    }
} 