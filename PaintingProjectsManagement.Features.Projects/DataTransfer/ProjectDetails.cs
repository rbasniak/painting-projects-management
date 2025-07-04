namespace PaintingProjectsManagement.Features.Projects;
public class ProjectDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public ProjectStepsDetails Steps { get; set; } = new();

    public MaterialDetails[] Materials { get; set; } = Array.Empty<MaterialDetails>();
    public UrlReference[] References { get; set; } = Array.Empty<UrlReference>();
    public UrlReference[] Pictures { get; set; } = Array.Empty<UrlReference>();
    public ColorSectionDetails[] Sections { get; set; } = Array.Empty<ColorSectionDetails>();

    public static ProjectDetails FromModel(Project project, MaterialDetails[]? materialDetails = null)
    {
        ArgumentNullException.ThrowIfNull(project);

        return new ProjectDetails
        {
            Id = project.Id,
            Name = project.Name,
            PictureUrl = project.PictureUrl,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Steps = ProjectStepsDetails.FromModel(project.Steps),
            Materials = materialDetails ?? Array.Empty<MaterialDetails>(),
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
            Sections = project.Sections
                .Select(ColorSectionDetails.FromModel)
                .ToArray()
        };
    }
}

public class UrlReference
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
}