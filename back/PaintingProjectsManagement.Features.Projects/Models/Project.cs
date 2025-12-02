namespace PaintingProjectsManagement.Features.Projects;

public class Project : TenantEntity
{
    private HashSet<MaterialForProject> _materials = new();
    private HashSet<ProjectReference> _references = new();
    private HashSet<ProjectPicture> _pictures = new();
    private HashSet<ColorGroup> _groups = new();
    private HashSet<ProjectStepData> _steps = new();

    // EF Core constructor, don't remote it
    private Project()
    {
        
    }

    public Project(string tenant, string name, DateTime startDate, Guid? modelId)
    {
        TenantId = tenant;
        Name = name;
        StartDate = startDate;
        ModelId = modelId ?? null;

        _materials = new HashSet<MaterialForProject>();
        _references = new HashSet<ProjectReference>();
        _pictures = new HashSet<ProjectPicture>();
        _steps = new HashSet<ProjectStepData>();
        _groups = new HashSet<ColorGroup>();
    }

    public string Name { get; private set; } = string.Empty;
    public string? PictureUrl { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public Guid? ModelId { get; private set; }

    public IEnumerable<MaterialForProject> Materials => _materials.AsReadOnly();
    public IEnumerable<ProjectReference> References => _references.AsReadOnly();
    public IEnumerable<ProjectPicture> Pictures => _pictures.AsReadOnly();
    public IEnumerable<ColorGroup> ColorGroups => _groups.AsReadOnly();
    public IEnumerable<ProjectStepData> Steps => _steps.AsReadOnly();

    public void UpdateDetails(string name, string pictureUrl, DateTime? startDate, DateTime? endDate)
    {
        Name = name;
        PictureUrl = pictureUrl;
        StartDate = startDate != null ? startDate.Value : StartDate;    
        EndDate = endDate;
    }

    public void ConsumeMaterial(Guid materialId, double quantity, MaterialUnit unit)
    {
        // TODO: make it idempotent, also:
        // User can send the same material multiple times, we should aggregate it
        // How to handle different units for the same material?

        _materials.Add(new MaterialForProject(Id, materialId, quantity, unit));

        RaiseDomainEvent(new ProjectMaterialAdded(Id, materialId, quantity));
    }

    public void AddExecutionWindow(ProjectStepDefinition step, DateTime start, DateTime end)
    {
        _steps.Add(new ProjectStepData(Id, step, start, end));
        RaiseDomainEvent(new BuildingStepAddedToTheProject(Id, (int)step, start, end));
    }

    public void AddExecutionWindow(ProjectStepDefinition step, DateTime start, double duration)
    {
        _steps.Add(new ProjectStepData(Id, step, start, duration));
        var end = start.AddHours(duration);
        RaiseDomainEvent(new BuildingStepAddedToTheProject(Id, (int)step, start, end));
    }

    internal double GetTotalWorkingHours()
    {
        if (_steps == null)
        {
            throw new InvalidOperationException($"Property {nameof(Steps)} is not loaded from the database");
        }

        return _steps
            .Where(x => x.Step is
                ProjectStepDefinition.Planning or
                ProjectStepDefinition.Supporting or
                ProjectStepDefinition.PostProcessing or 
                ProjectStepDefinition.Cleaning or 
                ProjectStepDefinition.Painting)
            .Sum(step => step.Duration);
    }

    internal double GetTotalPrintingTimeInHours()
    {
        if (_steps == null)
        {
            throw new InvalidOperationException($"Property {nameof(Steps)} is not loaded from the database");
        }

        return _steps
            .Where(x => x.Step is ProjectStepDefinition.Printing)
            .Sum(step => step.Duration);
    }
}