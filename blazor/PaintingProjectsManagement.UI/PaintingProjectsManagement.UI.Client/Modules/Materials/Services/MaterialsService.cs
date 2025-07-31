using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using PaintingProjectsManagement.UI.Client.Modules.Materials.Models;

namespace PaintingProjectsManagement.UI.Client.Modules.Materials.Services;

public class MaterialsService : IMaterialsService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public MaterialsService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
    }

    public async Task<IReadOnlyCollection<MaterialDetails>> GetMaterialsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<MaterialDetails>>($"{_baseUrl}/api/materials");
        return response ?? Array.Empty<MaterialDetails>();
    }

    public async Task<MaterialDetails> CreateMaterialAsync(CreateMaterialRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/materials", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MaterialDetails>() ?? new MaterialDetails();
    }

    public async Task<MaterialDetails> UpdateMaterialAsync(UpdateMaterialRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/materials", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MaterialDetails>() ?? new MaterialDetails();
    }

    public async Task DeleteMaterialAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/materials/{id}");
        response.EnsureSuccessStatusCode();
    }
} 