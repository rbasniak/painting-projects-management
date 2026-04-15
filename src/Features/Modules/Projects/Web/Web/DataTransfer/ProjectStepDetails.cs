using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectStepDetails
{
    public Guid Id { get; set; }
    public EnumReference Step { get; set; } = default!;
    public DateTime Date { get; set; }
    public double DurationInHours { get; set; }
}