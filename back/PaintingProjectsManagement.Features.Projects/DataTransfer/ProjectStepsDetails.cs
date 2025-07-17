namespace PaintingProjectsManagement.Features.Projects;

public class ProjectStepsDetails
{
    public Guid Id { get; set; }
    public ProjectStepDataDetails Planning { get; set; } = new();
    public ProjectStepDataDetails Painting { get; set; } = new();
    public ProjectStepDataDetails Preparation { get; set; } = new();
    public ProjectStepDataDetails Supporting { get; set; } = new();

    public static ProjectStepsDetails FromModel(ProjectSteps? steps)
    {
        if (steps == null)
        {
            return new ProjectStepsDetails();
        }

        return new ProjectStepsDetails
        {
            Id = steps.Id,
            Planning = ProjectStepDataDetails.FromModel(steps.Planning),
            Painting = ProjectStepDataDetails.FromModel(steps.Painting),
            Preparation = ProjectStepDataDetails.FromModel(steps.Preparation),
            Supporting = ProjectStepDataDetails.FromModel(steps.Supporting)
        };
    }
}

public class ProjectStepDataDetails
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public double Duration { get; set; }

    public static ProjectStepDataDetails FromModel(ProjectStepData? stepData)
    {
        if (stepData == null)
        {
            return new ProjectStepDataDetails();
        }

        return new ProjectStepDataDetails
        {
            Id = stepData.Id,
            Date = stepData.Date,
            Duration = stepData.Duration
        };
    }
}