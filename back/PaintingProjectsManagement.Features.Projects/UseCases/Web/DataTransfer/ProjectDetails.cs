namespace PaintingProjectsManagement.Features.Projects;
public class ProjectDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public ProjectStepDataDetails[] Steps { get; set; } = Array.Empty<ProjectStepDataDetails>();
    // public MaterialDetails[] Materials { get; set; } = Array.Empty<MaterialDetails>();
    public UrlReference[] References { get; set; } = Array.Empty<UrlReference>();
    public UrlReference[] Pictures { get; set; } = Array.Empty<UrlReference>();
    public ColorGroupDetails[] Groups { get; set; } = Array.Empty<ColorGroupDetails>();
    public ProjectCostDetails CostBreakdown { get; set; } = ProjectCostDetails.Empty;

    public static ProjectDetails FromModel(Project project, ProjectCostBreakdown projectCostBreakdown)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(projectCostBreakdown);

        return new ProjectDetails
        {
            Id = project.Id,
            Name = project.Name,
            PictureUrl = project.PictureUrl,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Steps = project.Steps.Select(x => ProjectStepDataDetails.FromModel(x)).ToArray(),
            // Materials = materialDetails ?? Array.Empty<MaterialDetails>(),
            CostBreakdown = ProjectCostDetails.FromModel(projectCostBreakdown),
            References = project.References.Select(r => new UrlReference
            {
                Id = r.Id,
                Url = r.Url
            }).ToArray(),
            Pictures = project.Pictures.Select(p => new UrlReference
            {
                Id = p.Id,
                Url = p.Url
            }).ToArray(),
            Groups = project.ColorGroups
                .Select(ColorGroupDetails.FromModel)
                .ToArray(),
            
        };
    }
}

public class UrlReference
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
}