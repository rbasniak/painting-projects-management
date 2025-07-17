namespace PaintingProjectsManagement.Features.Projects;

public class ColorGroup
{
    private HashSet<ColorSection> _sections = new();

    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public IEnumerable<ColorSection> Sections => _sections.AsReadOnly();
}