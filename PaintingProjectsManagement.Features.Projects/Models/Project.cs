namespace PaintingProjectsManagement.Features.Projects;

public class Project
{
    private HashSet<MaterialForProject> _materials = new();
    private HashSet<ProjectReference> _references = new();
    private HashSet<ProjectPicture> _pictures = new();
    private HashSet<ColorSection> _sections = new();

    // EF Core constructor, don't remote it
    private Project()
    {
        
    }

    public Project(Guid id, string name, string pictureUrl, DateTime startDate)
    {
        Id = id;
        Name = name;
        PictureUrl = pictureUrl;
        StartDate = startDate;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string PictureUrl { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public ProjectSteps Steps { get; private set; } = new();

    public IEnumerable<MaterialForProject> Materials => _materials.AsReadOnly();
    public IEnumerable<ProjectReference> References => _references.AsReadOnly();
    public IEnumerable<ProjectPicture> Pictures => _pictures.AsReadOnly();
    public IEnumerable<ColorSection> Sections => _sections.AsReadOnly();

    public void UpdateDetails(string name, string pictureUrl, DateTime startDate, DateTime? endDate)
    {
        Name = name;
        PictureUrl = pictureUrl;
        StartDate = startDate;
        EndDate = endDate;
    }
}