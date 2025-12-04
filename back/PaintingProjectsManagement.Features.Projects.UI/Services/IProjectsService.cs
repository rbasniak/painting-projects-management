using System.Net.Http.Json;

namespace PaintingProjectsManagement.UI.Modules.Projects;

public interface IProjectsService
{
    Task<IReadOnlyCollection<ProjectDetails>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProjectDetails> GetDetailsAsync(Guid projectId, CancellationToken cancellationToken);

    Task<ProjectDetails> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken);

    Task<ProjectDetails> UpdateAsync(UpdateProjectRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProjectMaterialDetails>> GetProjectMaterialsAsync(Guid projectId, CancellationToken cancellationToken);

    Task<ProjectStepsGrouped> GetProjectStepsAsync(Guid projectId, CancellationToken cancellationToken);

    Task UpdateProjectMaterialAsync(Guid projectId, Guid materialId, double quantity, int unit, CancellationToken cancellationToken);

    Task DeleteProjectMaterialAsync(Guid projectId, Guid materialId, CancellationToken cancellationToken);

    Task AddProjectStepAsync(Guid projectId, int step, DateTime date, double duration, CancellationToken cancellationToken);

    Task UpdateProjectStepAsync(Guid projectId, Guid stepId, DateTime? date, double? duration, CancellationToken cancellationToken);

    Task DeleteProjectStepAsync(Guid projectId, Guid stepId, CancellationToken cancellationToken);

    Task AddProjectMaterialAsync(Guid projectId, Guid materialId, double quantity, int unit, CancellationToken cancellationToken);
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

        if (result == null)
        {
            throw new Exception($"Project steps for project ID {projectId} not found in the response.");
        }

        return result;
    }

    public async Task UpdateProjectMaterialAsync(Guid projectId, Guid materialId, double quantity, int unit, CancellationToken cancellationToken)
    {
        var request = new { Quantity = quantity, Unit = unit };
        var response = await _httpClient.PutAsJsonAsync($"api/projects/{projectId}/materials/{materialId}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to update project material. Status code: {response.StatusCode}");
        }
    }

    public async Task DeleteProjectMaterialAsync(Guid projectId, Guid materialId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/projects/{projectId}/materials/{materialId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to delete project material. Status code: {response.StatusCode}");
        }
    }

    public async Task AddProjectStepAsync(Guid projectId, int step, DateTime date, double duration, CancellationToken cancellationToken)
    {
        var request = new { Step = step, Date = date, Duration = duration };
        var response = await _httpClient.PostAsJsonAsync($"api/projects/{projectId}/steps", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to add project step. Status code: {response.StatusCode}");
        }
    }

    public async Task UpdateProjectStepAsync(Guid projectId, Guid stepId, DateTime? date, double? duration, CancellationToken cancellationToken)
    {
        var request = new { Date = date, Duration = duration };
        var response = await _httpClient.PutAsJsonAsync($"api/projects/{projectId}/steps/{stepId}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to update project step. Status code: {response.StatusCode}");
        }
    }

    public async Task DeleteProjectStepAsync(Guid projectId, Guid stepId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"api/projects/{projectId}/steps/{stepId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to delete project step. Status code: {response.StatusCode}");
        }
    }

    public async Task AddProjectMaterialAsync(Guid projectId, Guid materialId, double quantity, int unit, CancellationToken cancellationToken)
    {
        var request = new { ProjectId = projectId, MaterialId = materialId, Quantity = quantity, Unit = unit };
        var response = await _httpClient.PostAsJsonAsync("api/projects/materials", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to add project material. Status code: {response.StatusCode}");
        }
    }
}