using PaintingProjectsManagement.Blazor.Features.Projects.Models;

namespace PaintingProjectsManagement.Blazor.Features.Projects.Services;

public interface IProjectsApiService
{
    Task<List<Project>> GetProjectsAsync();
    Task<Project?> GetProjectAsync(Guid id);
    Task<Project> CreateProjectAsync(CreateProjectRequest request);
    Task<Project> UpdateProjectAsync(Guid id, UpdateProjectRequest request);
    Task DeleteProjectAsync(Guid id);
}

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
}

public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
} 