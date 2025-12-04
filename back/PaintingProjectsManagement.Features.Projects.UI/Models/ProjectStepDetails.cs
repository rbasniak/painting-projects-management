using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Projects;

public class ProjectStepDetails
{
    public Guid Id { get; set; }
    public EnumReference Step { get; set; } = default!;
    public DateTime Date { get; set; }
    public double Duration { get; set; }
}
