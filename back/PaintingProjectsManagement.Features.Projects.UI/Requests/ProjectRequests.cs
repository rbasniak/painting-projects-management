namespace PaintingProjectsManagement.UI.Modules.Projects;

public enum ColorZone
{
    Midtone = 0,
    Highlight = 1,
    Shadow = 2,
}



// DOCS: Simple crud requests for create/update that don't differ much such be in the same file
public class CreateProjectRequest
{
    public string Name { get; init; } = string.Empty;
}

public class UpdateProjectRequest : CreateProjectRequest
{
    public Guid Id { get; init; }
}

public class CreateColorGroupRequest
{
    public Guid ProjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ColorZone[] Zones { get; init; } = Array.Empty<ColorZone>();
}

public class UpdateColorGroupRequest
{
    public Guid ProjectId { get; init; }
    public Guid ColorGroupId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ColorZone[] Zones { get; init; } = Array.Empty<ColorZone>();
}

public class DeleteColorGroupRequest
{
    public Guid ProjectId { get; init; }
    public Guid ColorGroupId { get; init; }
}

public class UpdateColorSectionRequest
{
    public Guid SectionId { get; init; }
    public string ReferenceColor { get; init; } = string.Empty;
}

public class UpdatePickedColorRequest
{
    public Guid SectionId { get; init; }
    public Guid PaintColorId { get; init; }
} 