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

public class AvailableProjectMaterialDetails
{
    public Guid MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DefaultUnit { get; set; }
}

public enum ExecutionMaterialUnit
{
    Drop = 1,
    Unit = 2,
    Centimeter = 3,
    Meter = 4,
    Gram = 5,
    Kilogram = 6,
    Liter = 7,
    Mililiter = 8,
    Spray = 9
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