using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectStepsGrouped
{
    // System.Text.Json only supports string keys for dictionary serialization.
    // Use the enum name as the key to avoid custom converters.
    public Dictionary<int, List<ProjectStepDetails>> StepsByType { get; set; } = new();
}