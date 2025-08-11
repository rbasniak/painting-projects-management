using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectStepDataDetails
{
    public Guid Id { get; set; }
    public EnumReference Step { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double Duration { get; set; }

    public static ProjectStepDataDetails FromModel(ProjectStepData step)
    {
        ArgumentNullException.ThrowIfNull(step);
        return new ProjectStepDataDetails
        {
            Id = step.Id,
            Step = new EnumReference(step.Step),
            StartDate = step.Date,
            EndDate = step.Date.AddHours(step.Duration),
            Duration = step.Duration
        };
    }
}
