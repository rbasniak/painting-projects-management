using rbkApiModules.Commons.Core.Database;
namespace PaintingProjectsManagement.Features.Projects;

public class ProjectSteps
{

    public ProjectStepData[] Planning { get; set; } = [];

    public ProjectStepData[] Supporting { get; set; } = [];

    public ProjectStepData[] Preparation { get; set; } = [];

    public ProjectStepData[] Painting { get; set; } = [];
}