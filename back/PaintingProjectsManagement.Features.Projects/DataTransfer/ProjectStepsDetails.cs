namespace PaintingProjectsManagement.Features.Projects;

public class ProjectStepsDetails
{
    public ProjectStepDataDetails[] Planning { get; set; } = [];
    public ProjectStepDataDetails[] Painting { get; set; } = [];
    public ProjectStepDataDetails[] Preparation { get; set; } = [];
    public ProjectStepDataDetails[] Supporting { get; set; } = [];

    public static ProjectStepsDetails FromModel(ProjectSteps steps)
    {
        ArgumentNullException.ThrowIfNull(steps);   

        return new ProjectStepsDetails
        {
            //Planning = ProjectStepDataDetails.FromModel(steps.Planning),
            //Painting = ProjectStepDataDetails.FromModel(steps.Painting),
            //Preparation = ProjectStepDataDetails.FromModel(steps.Preparation),
            //Supporting = ProjectStepDataDetails.FromModel(steps.Supporting)
        };
    }
}

public class ProjectStepDataDetails
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double Duration { get; set; }

    public static ProjectStepDataDetails[] FromModel(ProjectStepData[] stepData)
    {
        ArgumentNullException.ThrowIfNull(stepData);

        return stepData.Select(x => new ProjectStepDataDetails
        {
            Id = x.Id,
            EndDate = x.EndDate,
            StartDate = x.StartDate,
            Duration = x.Duration
        }).ToArray();
    }
}