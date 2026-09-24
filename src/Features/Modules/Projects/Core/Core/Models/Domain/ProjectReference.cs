namespace PaintingProjectsManagement.Features.Projects;

/// <summary>
/// Imagens do modelo 3D renderizado e imagens de referência da internet
/// </summary>
public class ProjectReference : BaseEntity
{
    private ProjectReference()
    {
        // EF Core constructor, don't remove it
    }

    public ProjectReference(Guid projectId, string url)
    {
        ProjectId = projectId;
        Url = url;
    }

    public Guid ProjectId { get; private set; }
    public string Url { get; private set; } = string.Empty;
}
