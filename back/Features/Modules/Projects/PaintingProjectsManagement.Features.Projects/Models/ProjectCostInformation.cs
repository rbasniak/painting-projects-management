namespace PaintingProjectsManagement.Features.Projects;

public class ProjectCostBreakdown : BaseEntity
{
    public required Guid ProjectId { get; init; }
    public required ElectricityCost Electricity { get; init; }
    public required Dictionary<string, LaborCost>  Labor { get; init; }
    public Dictionary<string, IReadOnlyCollection<MaterialsCost>> Materials { get; init; }
}

public class ElectricityCost
{
    public required Money CostPerKWh { get; init; }
    public required double PrinterConsumptionInKWh { get; init; }
    public required double TotalPrintingTimeInHours { get; init; }
    public Money TotalCost => new(CostPerKWh.Amount * PrinterConsumptionInKWh * TotalPrintingTimeInHours, CostPerKWh.Currency);

    public static ElectricityCost Empty() => new ElectricityCost
    {
        PrinterConsumptionInKWh = 0,
        TotalPrintingTimeInHours = 0,
        CostPerKWh = new Money(0, "USD")
    };
}

public class LaborCost
{
    public required double SpentHours { get; init; }
    public required Money CostPerHour { get; init; }
    public Money TotalCost => new(CostPerHour.Amount * SpentHours, CostPerHour.Currency);

    public static LaborCost Empty() => new LaborCost
    {
        SpentHours = 0,
        CostPerHour = new Money(0, "USD")
    };
}

public class MaterialsCost
{
    public required Guid MaterialId { get; init; }
    public required string Description { get; init; } = string.Empty;
    public required string Category { get; init; } = string.Empty;
    public required Quantity Quantity { get; init; }
    public required Money CostPerUnit { get; init; }
    public required double Markup { get; init; }
    public Money TotalCost => new(CostPerUnit.Amount * Quantity.Value * Markup, CostPerUnit.Currency);
}

