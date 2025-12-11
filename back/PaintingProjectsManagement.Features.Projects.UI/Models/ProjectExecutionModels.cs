using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Projects;

public class ProjectMaterialDetails
{
    public Guid MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string QuantityFormatted { get; set; } = string.Empty;
    public int Unit { get; set; }
}

public class ProjectStepDetails
{
    public Guid Id { get; set; }
    public EnumReference Step { get; set; } = new(0, string.Empty);
    public DateTime Date { get; set; }
    public double DurationInHours { get; set; }
}

public class ProjectStepsGrouped
{
    public Dictionary<int, List<ProjectStepDetails>> StepsByType { get; set; } = new();
}