using PaintingProjectsManagement.Features.Currency;

namespace PaintingProjectsManagement.Features.Projects;

public interface IProjectCostCalculator
{
    Task<ProjectCostBreakdown> CalculateCostAsync(Guid projectId, string currency, CancellationToken cancellationToken);
}

public class ProjectCostCalculator(
    DbContext context,
    ProjectSettings projectSettings,
    ICurrencyConverter currencyConverter,
    IUnitsConverter unitsConverter
) : IProjectCostCalculator
{
    public async Task<ProjectCostBreakdown> CalculateCostAsync(Guid projectId, string currency, CancellationToken cancellationToken)
    {
        var project = context.Set<Project>()
            .AsNoTracking()
            .Include(x => x.Materials)
            .Include(x => x.Steps)
            .First(x => x.Id == projectId);

        var projectMaterialsById = project.Materials.ToDictionary(x => x.MaterialId);
        var projectMaterialIds = projectMaterialsById.Keys.ToArray();

        var projectMaterialDefinitions = await context.Set<Material>()
            .AsNoTracking()
            .Where(x => x.Tenant == project.TenantId && projectMaterialIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var projectMaterials = projectMaterialDefinitions
            .GroupBy(x => x.Id)
            .Select(group => group.OrderByDescending(x => x.UpdatedUtc).First())
            .ToDictionary(
                x => x.Id,
                x => (MaterialDefinition: x, ProjectMaterial: projectMaterialsById[x.Id])
            );

        var costBreakdown = new ProjectCostBreakdown
        {
            ProjectId = project.Id,
            Electricity = new ElectricityCost
            {
                PrinterConsumptionInKWh = projectSettings.PrinterConsumptioninKwh,
                CostPerKWh = await projectSettings.ElectricityCostPerKwh.Convert(currency, currencyConverter),
                TotalPrintingTimeInHours = project.GetTotalPrintingTimeInHours()
            },
            Labor = new Dictionary<string, LaborCost>(),
            Materials = new Dictionary<string, IReadOnlyCollection<MaterialsCost>>()
        };

        foreach (var stepGroup in project.Steps.GroupBy(x => x.Step).OrderBy(x => (int)x.Key))
        {
            if (stepGroup.Key == ProjectStepDefinition.Printing)
            {
                continue;
            }

            var laborCost = new LaborCost
            {
                CostPerHour = await projectSettings.LaborCostPerHour.Convert(currency, currencyConverter),
                SpentHours = stepGroup.Sum(x => x.Duration),
            };

            costBreakdown.Labor.Add(stepGroup.Key.ToString(), laborCost);
        }

        // Group by CategoryId for resin waste, but bucket output by display name so duplicate CategoryName
        // values (bad projection data or enum/id drift) do not throw on Dictionary.Add.
        var materialsByCategoryName = new Dictionary<string, List<MaterialsCost>>(StringComparer.Ordinal);

        foreach (var categoryGroup in projectMaterials.GroupBy(x => x.Value.MaterialDefinition.CategoryId).OrderBy(x => x.Key))
        {
            var resinWasteFactor = categoryGroup.Key == 10 ? projectSettings.ResinWasteFactor : 1.0; // TODO: Dangerous hardcoded value (resins)

            var categoryName = categoryGroup.First().Value.MaterialDefinition.CategoryName;
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                categoryName = "Uncategorized";
            }

            if (!materialsByCategoryName.TryGetValue(categoryName, out var materialsCosts))
            {
                materialsCosts = new List<MaterialsCost>();
                materialsByCategoryName[categoryName] = materialsCosts;
            }

            foreach (var pm in categoryGroup)
            {
                var projectMaterial = pm.Value.ProjectMaterial;
                var materiaDefinition = pm.Value.MaterialDefinition;

                materialsCosts.Add(new MaterialsCost
                {
                    MaterialId = pm.Key,
                    Description = materiaDefinition.Name,
                    Category = materiaDefinition.CategoryName,
                    Quantity = unitsConverter.Convert(projectMaterial.Quantity * resinWasteFactor, materiaDefinition.Unit),
                    Markup = projectSettings.MaterialMarkup,
                    CostPerUnit = await materiaDefinition.PricePerUnit.Convert(currency, currencyConverter)
                });
            }
        }

        foreach (var (name, list) in materialsByCategoryName)
        {
            costBreakdown.Materials.Add(name, list.AsReadOnly());
        }

        return costBreakdown;
    }
}
