namespace PaintingProjectsManagement.Features.Projects;

/// <summary>
/// Fotos do projeto finalizado
/// </summary>
public class ProjectPicture : BaseEntity
{
    private ProjectPicture()
    {
        // EF Core constructor, don't remove it
    }

    public ProjectPicture(Guid projectId, string url)
    {
        ProjectId = projectId;
        Url = url;
    }
    public Guid ProjectId { get; private set; }
    public string Url { get; private set; } = string.Empty;
}