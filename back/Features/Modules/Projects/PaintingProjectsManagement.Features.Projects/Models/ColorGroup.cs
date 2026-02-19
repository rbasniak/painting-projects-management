namespace PaintingProjectsManagement.Features.Projects;

public class ColorGroup : BaseEntity
{
    private HashSet<ColorSection> _sections = new();

    private ColorGroup()
    {
        // EF Core constructor, don't remove it
    }

    public ColorGroup(Project project, string name)
    {
        Project = project;
        Name = name;

        _sections = [];
    }

    public Project Project { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public IEnumerable<ColorSection> Sections => _sections.AsReadOnly();

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        Name = name;
    }

    public void AddSection(ColorZone zone, string referenceColor)
    {
        if (_sections.Any(x => x.Zone == zone))
        {
            throw new InvalidOperationException($"Section with zone {zone} already exists.");
        }

        var section = new ColorSection(Id, zone, referenceColor);
        _sections.Add(section);
    }

    public void UpdateSection(ColorZone zone, string referenceColor)
    {
        var section = _sections.FirstOrDefault(x => x.Zone == zone);
        if (section == null)
        {
            throw new InvalidOperationException($"Section with zone {zone} not found.");
        }

        section.UpdateReferenceColor(referenceColor);
    }

    public void RemoveSection(ColorZone zone)
    {
        var section = _sections.FirstOrDefault(x => x.Zone == zone);
        if (section != null)
        {
            _sections.Remove(section);
        }
    }
}