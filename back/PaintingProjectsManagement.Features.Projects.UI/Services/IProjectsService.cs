using System.Net.Http.Json;

namespace PaintingProjectsManagement.UI.Modules.Projects;

public interface IProjectsService
{
    Task<IReadOnlyCollection<ProjectDetails>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProjectDetails> GetDetailsAsync(Guid projectId, CancellationToken cancellationToken);

    Task<ProjectDetails> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken);

    Task<ProjectDetails> UpdateAsync(UpdateProjectRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    // Execution management
    Task<IReadOnlyCollection<ProjectMaterialDetails>> GetProjectMaterialsAsync(Guid projectId, CancellationToken cancellationToken);

    Task<ProjectStepsGrouped> GetProjectStepsAsync(Guid projectId, CancellationToken cancellationToken);

    Task UpdateProjectMaterialAsync(UpdateProjectMaterialRequest request, CancellationToken cancellationToken);

    Task DeleteProjectMaterialAsync(Guid projectId, Guid materialId, CancellationToken cancellationToken);

    Task AddProjectMaterialAsync(AddProjectMaterialRequest request, CancellationToken cancellationToken);

    Task AddProjectStepAsync(AddProjectStepRequest request, CancellationToken cancellationToken);

    Task UpdateProjectStepAsync(UpdateProjectStepRequest request, CancellationToken cancellationToken);

    Task DeleteProjectStepAsync(Guid projectId, Guid stepId, CancellationToken cancellationToken);
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

    public async Task<IReadOnlyCollection<ProjectMaterialDetails>> GetProjectMaterialsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/projects/{projectId}/execution/materials", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get project materials for project ID {projectId}. Status code: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ProjectMaterialDetails>>();
        return result ?? Array.Empty<ProjectMaterialDetails>();
    }

    public async Task<ProjectStepsGrouped> GetProjectStepsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"api/projects/{projectId}/execution/steps", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get project steps for project ID {projectId}. Status code: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<ProjectStepsGrouped>();
        return result ?? new ProjectStepsGrouped();
    }

    public async Task UpdateProjectMaterialAsync(UpdateProjectMaterialRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/projects/{request.ProjectId}/materials/{request.MaterialId}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteProjectMaterialAsync(Guid projectId, Guid materialId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/projects/{projectId}/materials/{materialId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task AddProjectMaterialAsync(AddProjectMaterialRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/projects/materials", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task AddProjectStepAsync(AddProjectStepRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/projects/{request.ProjectId}/steps", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateProjectStepAsync(UpdateProjectStepRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/projects/{request.ProjectId}/steps/{request.StepId}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteProjectStepAsync(Guid projectId, Guid stepId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/projects/{projectId}/steps/{stepId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}