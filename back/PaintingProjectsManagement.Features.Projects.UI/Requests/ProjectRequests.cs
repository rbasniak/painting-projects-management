namespace PaintingProjectsManagement.UI.Modules.Projects;

// DOCS: Simple crud requests for create/update that don't differ much such be in the same file
public class CreateProjectRequest
{
    public string Name { get; init; } = string.Empty;
}

public class UpdateProjectRequest : CreateProjectRequest
{
    public Guid Id { get; init; }
} 