using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectStepDetails
{
    public Guid Id { get; set; }
    public EnumReference Step { get; set; } = default!;
    public DateTime Date { get; set; }
    public string DateFormatted { get; set; } = string.Empty;
    public double DurationInHours { get; set; }
    public string DurationFormatted { get; set; } = string.Empty;
}