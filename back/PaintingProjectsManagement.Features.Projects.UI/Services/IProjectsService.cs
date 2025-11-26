using System.Net.Http.Json;

namespace PaintingProjectsManagement.UI.Modules.Projects;

public interface IProjectsService
{
    Task<IReadOnlyCollection<ProjectsDetails>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProjectsDetails> CreateAsync(
      CreateProjectRequest request,
      CancellationToken cancellationToken);

    Task<ProjectsDetails> UpdateAsync(
      UpdateProjectRequest request,
      CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public class ProjectsService : IProjectsService
{
    private readonly HttpClient _httpClient;

    public ProjectsService(HttpClient httpClient) => this._httpClient = httpClient;

    public async Task<IReadOnlyCollection<ProjectsDetails>> GetAllAsync(
      CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/projects", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ProjectsDetails>>();
            return result ?? Array.Empty<ProjectsDetails>();
        }

        return Array.Empty<ProjectsDetails>();
    }

    public async Task<ProjectsDetails> CreateAsync(
      CreateProjectRequest request,
      CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/materials", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ProjectsDetails>();
            return result ?? new ProjectsDetails();
        }

        return new ProjectsDetails();
    }

    public async Task<ProjectsDetails> UpdateAsync(
      UpdateProjectRequest request,
      CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync("api/materials", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ProjectsDetails>();
            return result ?? new ProjectsDetails();
        }

        return new ProjectsDetails();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _httpClient.DeleteAsync($"api/materials/{id}", cancellationToken);
    }
}