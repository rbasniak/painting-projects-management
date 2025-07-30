namespace PaintingProjectsManagement.Features.Projects;

public class ColorGroup : BaseEntity
{
    private HashSet<ColorSection> _sections = new();

    private ColorGroup()
    {
        // EF Core constructor, don't remove it
    }

    public ColorGroup(Guid projectId, string name)
    {
        ProjectId = projectId;
        Name = name;

        _sections = [];
    }

    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public IEnumerable<ColorSection> Sections => _sections.AsReadOnly();
}