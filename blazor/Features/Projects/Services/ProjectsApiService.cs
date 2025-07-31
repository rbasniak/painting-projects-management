using System.Net.Http.Json;
using PaintingProjectsManagement.Blazor.Features.Projects.Models;

namespace PaintingProjectsManagement.Blazor.Features.Projects.Services;

public class ProjectsApiService : IProjectsApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://localhost:7236";

    public ProjectsApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_baseUrl);
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

    public async Task<Project?> GetProjectAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Project>($"/api/projects/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching project {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<Project> CreateProjectAsync(CreateProjectRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/projects", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Project>() ?? new Project();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating project: {ex.Message}");
            throw;
        }
    }

    public async Task<Project> UpdateProjectAsync(Guid id, UpdateProjectRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/projects/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Project>() ?? new Project();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating project {id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteProjectAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/projects/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting project {id}: {ex.Message}");
            throw;
        }
    }
} 