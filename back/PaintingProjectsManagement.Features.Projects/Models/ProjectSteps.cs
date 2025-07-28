namespace PaintingProjectsManagement.Features.Projects;

public class ProjectSteps : BaseEntity
{
    public Guid ProjectId { get; private set; }
    public ProjectStepData Planning { get; private set; } = new();
    public ProjectStepData Painting { get; private set; } = new();
    public ProjectStepData Preparation { get; private set; } = new();
    public ProjectStepData Supporting { get; private set; } = new();
}