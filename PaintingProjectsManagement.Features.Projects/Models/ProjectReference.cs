namespace PaintingProjectsManagement.Features.Projects;

/// <summary>
/// Imagens do modelo 3D renderizado e imagens de referência da internet
/// </summary>
public class ProjectReference
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Url { get; private set; } = string.Empty;
}