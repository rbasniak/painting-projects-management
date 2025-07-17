namespace PaintingProjectsManagement.Features.Projects;

public class ProjectHeader
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }

    public static ProjectHeader FromModel(Project project)
    {
        if (project == null)
            return new ProjectHeader();

        return new ProjectHeader
        {
            Id = project.Id,
            Name = project.Name,
            PictureUrl = project.PictureUrl,
            EndDate = project.EndDate
        };
    }
}