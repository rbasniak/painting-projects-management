using System.Net.Http.Json;
using PaintingProjectsManagement.Features.Inventory;
using PaintingProjectsManagement.UI.Modules.Projects;

namespace PaintingProjectsManagement.UI.Modules.Projects;

public interface IProjectsService
{
    Task<IReadOnlyCollection<ProjectDetails>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProjectDetails> GetDetailsAsync(Guid projectId, CancellationToken cancellationToken, string? currency = null);

    Task<IReadOnlyCollection<CurrencyOption>> GetCurrenciesAsync(CancellationToken cancellationToken);

    Task<ProjectDetails> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken);

    Task<ProjectDetails> UpdateAsync(UpdateProjectRequest request, CancellationToken cancellationToken);

    Task<ProjectDetails> ArchiveAsync(Guid id, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    // Execution management
    Task<IReadOnlyCollection<ProjectMaterialDetails>> GetProjectMaterialsAsync(Guid projectId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AvailableProjectMaterialDetails>> GetExecutionMaterialsCatalogAsync(Guid projectId, CancellationToken cancellationToken);

    Task<ProjectStepsGrouped> GetProjectStepsAsync(Guid projectId, CancellationToken cancellationToken);

    Task UpdateProjectMaterialAsync(UpdateProjectMaterialRequest request, CancellationToken cancellationToken);

    Task DeleteProjectMaterialAsync(Guid projectId, Guid materialId, CancellationToken cancellationToken);

    Task AddProjectMaterialAsync(AddProjectMaterialRequest request, CancellationToken cancellationToken);

    Task AddProjectStepAsync(AddProjectStepRequest request, CancellationToken cancellationToken);

    Task UpdateProjectStepAsync(UpdateProjectStepRequest request, CancellationToken cancellationToken);

    Task DeleteProjectStepAsync(Guid projectId, Guid stepId, CancellationToken cancellationToken);

    // Color groups management
    Task<ColorGroupDetails> CreateColorGroupAsync(CreateColorGroupRequest request, CancellationToken cancellationToken);

    Task<ColorGroupDetails> UpdateColorGroupAsync(UpdateColorGroupRequest request, CancellationToken cancellationToken);

    Task DeleteColorGroupAsync(DeleteColorGroupRequest request, CancellationToken cancellationToken);

    Task UpdateColorSectionAsync(UpdateColorSectionRequest request, CancellationToken cancellationToken);

    // Color matching
    Task MatchPaintsAsync(Guid projectId, MatchPaintsRequest request, CancellationToken cancellationToken);
    Task UpdatePickedColorAsync(UpdatePickedColorRequest request, CancellationToken cancellationToken);

    // Reference pictures
    Task<UrlReference[]> UploadReferencePictureAsync(UploadProjectReferencePictureRequest request, CancellationToken cancellationToken);
    Task DeleteReferencePictureAsync(Guid projectId, string pictureUrl, CancellationToken cancellationToken);
}

public class ProjectsService : IProjectsService
{
    private static readonly IReadOnlyCollection<CurrencyOption> DefaultCurrencies =
    [
        new CurrencyOption { Code = "DKK", Name = "Danish Krone" }
    ];

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
        var response = await _httpClient.PostAsJsonAsync("api/projects", request, cancellationToken);

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
        var response = await _httpClient.PutAsJsonAsync("api/projects", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ProjectDetails>();
            return result ?? new ProjectDetails();
        }

        return new ProjectDetails();
    }

    public async Task<ProjectDetails> ArchiveAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync($"api/projects/{id}/archive", null, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ProjectDetails>();
            return result ?? new ProjectDetails();
        }

        return new ProjectDetails();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _httpClient.DeleteAsync($"api/projects/{id}", cancellationToken);
    }

    public async Task<ProjectDetails> GetDetailsAsync(Guid projectId, CancellationToken cancellationToken, string? currency = null)
    {
        var requestUri = $"api/projects/{projectId}";

        if (!string.IsNullOrWhiteSpace(currency))
        {
            var normalizedCurrency = Uri.EscapeDataString(currency.Trim().ToUpperInvariant());
            requestUri = $"{requestUri}?currency={normalizedCurrency}";
        }

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

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

    public async Task<IReadOnlyCollection<CurrencyOption>> GetCurrenciesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/currencies", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return DefaultCurrencies;
            }

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<CurrencyOption>>(cancellationToken: cancellationToken);

            var currencies = result?
                .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                .Select(x => x with { Code = x.Code.Trim().ToUpperInvariant() })
                .OrderBy(x => x.Code, StringComparer.Ordinal)
                .ToArray();

            return currencies is { Length: > 0 } ? currencies : DefaultCurrencies;
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return DefaultCurrencies;
        }
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

    public async Task<IReadOnlyCollection<AvailableProjectMaterialDetails>> GetExecutionMaterialsCatalogAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/projects/execution/materials/available", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get materials catalog for project ID {projectId}. Status code: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<AvailableProjectMaterialDetails>>();
        return result ?? Array.Empty<AvailableProjectMaterialDetails>();
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

    public async Task<ColorGroupDetails> CreateColorGroupAsync(CreateColorGroupRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/projects/color-groups", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ColorGroupDetails>();
        return result ?? throw new Exception("Failed to deserialize ColorGroupDetails from response.");
    }

    public async Task<ColorGroupDetails> UpdateColorGroupAsync(UpdateColorGroupRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync("api/projects/color-groups", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ColorGroupDetails>();
        return result ?? throw new Exception("Failed to deserialize ColorGroupDetails from response.");
    }

    public async Task DeleteColorGroupAsync(DeleteColorGroupRequest request, CancellationToken cancellationToken)
    {
        // For DELETE with body, we need to use a custom request
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "api/projects/color-groups")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateColorSectionAsync(UpdateColorSectionRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync("api/projects/color-sections/reference-color", request, cancellationToken);
                                                        
        response.EnsureSuccessStatusCode();
    }

    public async Task MatchPaintsAsync(Guid projectId, MatchPaintsRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/projects/{projectId}/color-sections/match-paints",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdatePickedColorAsync(UpdatePickedColorRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync("api/projects/color-sections/picked-color", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<UrlReference[]> UploadReferencePictureAsync(UploadProjectReferencePictureRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/projects/reference-picture", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UrlReference[]>();
            return result ?? Array.Empty<UrlReference>();
        }

        return Array.Empty<UrlReference>();
    }

    public async Task DeleteReferencePictureAsync(Guid projectId, string pictureUrl, CancellationToken cancellationToken)
    {
        var request = new { ProjectId = projectId, PictureUrl = pictureUrl };
        await _httpClient.PostAsJsonAsync("api/projects/reference-picture/delete", request, cancellationToken);
    }
}
