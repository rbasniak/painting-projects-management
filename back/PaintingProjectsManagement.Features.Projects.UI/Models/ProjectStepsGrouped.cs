using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Projects;

public class ProjectStepsGrouped
{
    public Dictionary<EnumReference, List<ProjectStepDetails>> StepsByType { get; set; } = new();
}
