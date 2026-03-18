namespace PaintingProjectsManagement.UI.Modules.Projects;

public class UpdateProjectMaterialRequest
{
    public Guid ProjectId { get; init; }
    public Guid MaterialId { get; init; }
    public double Quantity { get; init; }
    public int Unit { get; init; } // MaterialUnit enum value
}

public class AddProjectMaterialRequest
{
    public Guid ProjectId { get; init; }
    public Guid MaterialId { get; init; }
    public double Quantity { get; init; }
    public int Unit { get; init; } // MaterialUnit enum value
}

public class AddProjectStepRequest
{
    public Guid ProjectId { get; init; }
    public int Step { get; init; } // ProjectStepDefinition enum value
    public DateTime Date { get; init; }
    public double DurationInHours { get; init; }
}

public class UpdateProjectStepRequest
{
    public Guid ProjectId { get; init; }
    public Guid StepId { get; init; }
    public DateTime? Date { get; init; }
    public double? DurationInHours { get; init; }
}