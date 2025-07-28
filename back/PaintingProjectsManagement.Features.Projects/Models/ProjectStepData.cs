namespace PaintingProjectsManagement.Features.Projects;

public class ProjectStepData : BaseEntity
{
    public Guid StepId { get; private set; }
    public DateTime Date { get; private set; }
    public double Duration { get; private set; }
}