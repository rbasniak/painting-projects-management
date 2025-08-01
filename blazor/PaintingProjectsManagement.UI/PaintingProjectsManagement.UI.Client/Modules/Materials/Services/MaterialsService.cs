using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PaintingProjectsManagement.UI.Client.Modules.Materials.Models;
using PaintingProjectsManagement.UI.Client.Services;

namespace PaintingProjectsManagement.UI.Client.Modules.Materials.Services;

public class MaterialsService : IMaterialsService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly IAuthenticationService _authService;

    public MaterialsService(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7236";
        _authService = authService;
    }

    public async Task<IReadOnlyCollection<MaterialDetails>> GetMaterialsAsync()
    {
        await AddAuthHeader();
        var response = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<MaterialDetails>>($"{_baseUrl}/api/materials");
        return response ?? Array.Empty<MaterialDetails>();
    }

    public async Task<MaterialDetails> CreateMaterialAsync(CreateMaterialRequest request)
    {
        await AddAuthHeader();
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/materials", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MaterialDetails>() ?? new MaterialDetails();
    }

    public async Task<MaterialDetails> UpdateMaterialAsync(UpdateMaterialRequest request)
    {
        await AddAuthHeader();
        var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/materials", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MaterialDetails>() ?? new MaterialDetails();
    }

    public async Task DeleteMaterialAsync(Guid id)
    {
        await AddAuthHeader();
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/materials/{id}");
        response.EnsureSuccessStatusCode();
    }

    private async Task AddAuthHeader()
    {
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
} 