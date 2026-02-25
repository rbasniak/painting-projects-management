namespace PaintingProjectsManagement.Features.Projects;

public class ProjectCostDetails
{
    public required Guid Id { get; init; }
    public required ElectricityCostDetails Electricity { get; init; }
    public Dictionary<string, LaborCostDetails> Labor { get; init; }
    public Dictionary<string, IReadOnlyCollection<MaterialsCostDetails>> Materials { get; init; }

    public static ProjectCostDetails FromModel(ProjectCostBreakdown costBreakdown)
    {
        ArgumentNullException.ThrowIfNull(costBreakdown);
        
        return new ProjectCostDetails
        {
            Id = costBreakdown.Id,
            Electricity = ElectricityCostDetails.FromModel(costBreakdown.Electricity),
            Labor = costBreakdown.Labor?.ToDictionary(
                kvp => kvp.Key,
                kvp => LaborCostDetails.FromModel(kvp.Value)
            ) ?? new Dictionary<string, LaborCostDetails>(),
            Materials = costBreakdown.Materials?.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyCollection<MaterialsCostDetails>)kvp.Value
                    .Select(MaterialsCostDetails.FromModel)
                    .ToArray()
            ) ?? new Dictionary<string, IReadOnlyCollection<MaterialsCostDetails>>()
        };
    }

    public static ProjectCostDetails Empty => new ProjectCostDetails
    {
        Id = Guid.Empty,
        Electricity = ElectricityCostDetails.FromModel(ElectricityCost.Empty()),
        Labor = new Dictionary<string, LaborCostDetails>(),
        Materials = new Dictionary<string, IReadOnlyCollection<MaterialsCostDetails>>()
    };
}

public class MaterialsCostDetails
{
    public required Guid MaterialId { get; init; }
    public required string Description { get; init; }
    public required double Quantity { get; init; }
    public required string Category { get; init; }
    public required MoneyDetails TotalCost { get; init; }

    public static MaterialsCostDetails FromModel(MaterialsCost materialsCost)
    {
        ArgumentNullException.ThrowIfNull(materialsCost);
        
        return new MaterialsCostDetails
        {
            MaterialId = materialsCost.MaterialId,
            Description = materialsCost.Description,
            Quantity = materialsCost.Quantity.Value,
            Category = materialsCost.Category,
            TotalCost = MoneyDetails.FromModel(materialsCost.TotalCost)
        };
    }
}

public class LaborCostDetails
{
    public required double SpentHours { get; init; }
    public required MoneyDetails TotalCost { get; init; }
    public static LaborCostDetails FromModel(LaborCost laborCost)
    {
        ArgumentNullException.ThrowIfNull(laborCost);
        
        return new LaborCostDetails
        {
            SpentHours = laborCost.SpentHours,
            TotalCost = MoneyDetails.FromModel(laborCost.TotalCost)
        };
    }
}

public class ElectricityCostDetails
{
    public required MoneyDetails CostPerKWh { get; init; }
    public required double PrintingTimeInHours { get; init; }
    public required MoneyDetails TotalCost { get; init; }
    public static ElectricityCostDetails FromModel(ElectricityCost electricityCost)
    {
        ArgumentNullException.ThrowIfNull(electricityCost);
        
        return new ElectricityCostDetails
        {
            CostPerKWh = MoneyDetails.FromModel(electricityCost.CostPerKWh),
            PrintingTimeInHours = electricityCost.TotalPrintingTimeInHours,
            TotalCost = MoneyDetails.FromModel(electricityCost.TotalCost)
        };
    }
}

public class MoneyDetails
{
    public required double Amount { get; init; }
    public required string Currency { get; init; }

    public static MoneyDetails FromModel(Money money)
    {
        return new MoneyDetails
        {
            Amount = money.Amount,
            Currency = money.Currency
        };
    }

}