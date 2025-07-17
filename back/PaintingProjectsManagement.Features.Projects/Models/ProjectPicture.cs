namespace PaintingProjectsManagement.Features.Projects;

/// <summary>
/// Fotos do projeto finalizado
/// </summary>
public class ProjectPicture
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Url { get; private set; } = string.Empty;
}