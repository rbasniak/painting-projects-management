using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Projects;



public class ProjectStepDetails
{
    public Guid Id { get; set; }
    public EnumReference Step { get; set; } = new(0, string.Empty);
    public DateTime Date { get; set; }
    public string DateFormatted { get; set; } = string.Empty;
    public double DurationInHours { get; set; }
    public string DurationFormatted { get; set; } = string.Empty;
}

public class ProjectStepsGrouped
{
    public Dictionary<EnumReference, List<ProjectStepDetails>> StepsByType { get; set; } = new();
}