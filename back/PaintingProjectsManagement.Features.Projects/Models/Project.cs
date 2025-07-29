namespace PaintingProjectsManagement.Features.Projects;

public class Project : TenantEntity
{
    private HashSet<MaterialForProject> _materials = new();
    private HashSet<ProjectReference> _references = new();
    private HashSet<ProjectPicture> _pictures = new();
    private HashSet<ColorGroup> _groups = new();

    // EF Core constructor, don't remote it
    private Project()
    {
        
    }

    public Project(string tenant, string name, string pictureUrl, DateTime startDate)
    {
        TenantId = tenant;
        Name = name;
        PictureUrl = pictureUrl;
        StartDate = startDate;
    }

    public string Name { get; private set; } = string.Empty;
    public string PictureUrl { get; private set; } = string.Empty;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public ProjectSteps Steps { get; private set; } = new();

    public IEnumerable<MaterialForProject> Materials => _materials.AsReadOnly();
    public IEnumerable<ProjectReference> References => _references.AsReadOnly();
    public IEnumerable<ProjectPicture> Pictures => _pictures.AsReadOnly();
    public IEnumerable<ColorGroup> Groups => _groups.AsReadOnly();

    public void UpdateDetails(string name, string pictureUrl, DateTime startDate, DateTime? endDate)
    {
        Name = name;
        PictureUrl = pictureUrl;
        StartDate = startDate;
        EndDate = endDate;
    }
}