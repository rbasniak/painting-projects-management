namespace PaintingProjectsManagement.Features.Projects;

public class ProjectStepData
{
    private ProjectStepData()
    {
        // EF Core constructor, don't remove it
    }

    public ProjectStepData(Guid projectId, ProjectStepDefinition step, DateTime date, double durationInHours)
    {
        ProjectId = projectId;
        Step = step;
        Date = date;
        Duration = durationInHours;
    }

    public ProjectStepData(Guid projectId, ProjectStepDefinition step, DateTime startDate, DateTime endDate)
    {
        ProjectId = projectId;
        Step = step;
        Date = startDate;
        Duration = (endDate - startDate).TotalHours;
    }

    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public ProjectStepDefinition Step { get; private set; }
    public DateTime Date { get; private set; }
    public double Duration { get; private set; }

    public void Update(DateTime? date, double? duration)
    {
        if (date.HasValue)
        {
            Date = date.Value;
        }
        if (duration.HasValue)
        {
            Duration = duration.Value;
        }
    }
}