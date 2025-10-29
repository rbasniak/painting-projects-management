namespace PaintingProjectsManagement.Features.Projects;

public class ProjectCostBreakdown : BaseEntity
{
    public required Guid ProjectId { get; init; }
    public required ElectricityCost Electricity { get; init; }
    public required LaborCost Labor { get; init; }
    public Dictionary<string, IReadOnlyCollection<MaterialsCost>> Materials { get; init; }
}

public class ElectricityCost
{
    public required Money CostPerKWh { get; init; }
    public required double ConsumptionInKWh { get; init; }
    public Money TotalCost => new(CostPerKWh.Amount * ConsumptionInKWh, CostPerKWh.Currency);

    public static ElectricityCost Empty() => new ElectricityCost
    {
        ConsumptionInKWh = 0,
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
    public required Quantity Quantity { get; init; }
    public required Money CostPerUnit { get; init; }
    public Money TotalCost => new(CostPerUnit.Amount * Quantity.Value, CostPerUnit.Currency);
}

public class  ProjectCostCalculator(
    DbContext context,
    ICurrencyConverter currencyConverter
)
{
    public ProjectCostBreakdown CalculateCost(Guid projectId, string currency)
    {
        var project = context.Set<Project>()
            .AsNoTracking()
            .Include(x => x.Materials)
            .Include(x => x.Steps)
            .ThenInclude(x => x.Step)
            .First(p => p.Id == projectId);

        var projectMaterialsById = project.Materials.ToDictionary(x => x.MaterialId);

        var projectMaterials = context.Set<ReadOnlyMaterial>()
            .AsNoTracking()
            .Where(material => project.Materials.Select(projectMaterial => projectMaterial.MaterialId).Contains(material.Id))
            .ToDictionary(
                material => material.Id,
                material => (MaterialDefinition: material, ProjectMaterial: projectMaterialsById[material.Id])
            );

        var costBreakdown = new ProjectCostBreakdown
        {
            ProjectId = project.Id,
            Electricity = new ElectricityCost
            {
                ConsumptionInKWh = ProjectSettings.PrinterConsumptioninKwh,
                CostPerKWh = ICurrencyConverter.Convert(ProjectSettings.ElectricityCostPerKwh, currency),
            },
            Labor = new LaborCost
            {
                SpentHours = project.GetSpentHours(),
                CostPerHour = ICurrencyConverter.Convert(ProjectSettings.LaborCostPerHour, currency),
            },
            Materials = projectMaterials
                .GroupBy(x => x.Value.MaterialDefinition.CategoryId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(pm =>
                    {
                        var projectMaterial = pm.Value.ProjectMaterial;
                        var materiaDefinition = pm.Value.MaterialDefinition;
                        return new MaterialsCost { 
                            MaterialId = pm.Key,
                            Description = materiaDefinition.Name,
                            Quantity = IUnitConverter.Convert(projectMaterial.Quantity, materiaDefinition.Unit),
                            CostPerUnit = ICurrencyConverter.Convert(materiaDefinition.PricePerUnit, currency)
                        };
                    }).ToList().AsReadOnly()
                )
        };

        // Placeholder implementation
        return new ProjectCostBreakdown
        {
            ProjectId = projectId,
            Electricity = ElectricityCost.Empty(),
            Labor = LaborCost.Empty(),
            Materials = new Dictionary<string, IReadOnlyCollection<MaterialsCost>>()
        };
    }
}