using System.Net.Http.Json;

namespace PaintingProjectsManagement.UI.Modules.Projects;

public interface IProjectsService
{
    Task<IReadOnlyCollection<ProjectDetails>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProjectDetails> GetDetailsAsync(Guid projectId, CancellationToken cancellationToken);

    Task<ProjectDetails> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken);

    Task<ProjectDetails> UpdateAsync(UpdateProjectRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public class ProjectsService : IProjectsService
{
    private readonly HttpClient _httpClient;

    public ProjectsService(HttpClient httpClient) => this._httpClient = httpClient;

    public async Task<IReadOnlyCollection<ProjectDetails>> GetAllAsync(
      CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/projects", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ProjectDetails>>();
            return result ?? Array.Empty<ProjectDetails>();
        }

        return Array.Empty<ProjectDetails>();
    }

    public async Task<ProjectDetails> CreateAsync(
      CreateProjectRequest request,
      CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/materials", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ProjectDetails>();
            return result ?? new ProjectDetails();
        }

        return new ProjectDetails();
    }

    public async Task<ProjectDetails> UpdateAsync(
      UpdateProjectRequest request,
      CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync("api/materials", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ProjectDetails>();
            return result ?? new ProjectDetails();
        }

        return new ProjectDetails();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _httpClient.DeleteAsync($"api/materials/{id}", cancellationToken);
    }

    public async Task<ProjectDetails> GetDetailsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/projects/{projectId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get project details for project ID {projectId}. Status code: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<ProjectDetails>();

        if (result == null)
        {
            throw new Exception($"Project details for project ID {projectId} not found in the response.");
        }

        return result;
    }
}