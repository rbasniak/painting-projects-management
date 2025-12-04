using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectStepsGrouped
{
    public Dictionary<EnumReference, List<ProjectStepDetails>> StepsByType { get; set; } = new();
}
