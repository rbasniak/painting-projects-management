using PaintingProjectsManagement.Features.Currency;

namespace PaintingProjectsManagement.Features.Projects;

internal class ProjectCostCalculator(
    DbContext context,
    ProjectSettings projectSettings,
    ICurrencyConverter currencyConverter,
    IUnitsConverter unitsConverter
)
{
    public async Task<ProjectCostBreakdown> CalculateCost(Guid projectId, string currency, CancellationToken cancellationToken)
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
                PrinterConsumptionInKWh = projectSettings.PrinterConsumptioninKwh,
                CostPerKWh = await projectSettings.ElectricityCostPerKwh.Convert(currency, currencyConverter),
                TotalPrintingTimeInHours = project.GetTotalPrintingTimeInHours()
            },
            Labor = new LaborCost
            {
                SpentHours = project.GetTotalWorkingHours(),
                CostPerHour = await projectSettings.LaborCostPerHour.Convert(currency, currencyConverter),
            },
            Materials = await projectMaterials
                .GroupBy(x => x.Value.MaterialDefinition.CategoryId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(async pm =>
                    {
                        var projectMaterial = pm.Value.ProjectMaterial;
                        var materiaDefinition = pm.Value.MaterialDefinition;
                        return new MaterialsCost
                        {
                            MaterialId = pm.Key,
                            Description = materiaDefinition.Name,
                            Quantity = unitsConverter.Convert(projectMaterial.Quantity, materiaDefinition.Unit),
                            CostPerUnit = await materiaDefinition.PricePerUnit.Convert(currency, currencyConverter)
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