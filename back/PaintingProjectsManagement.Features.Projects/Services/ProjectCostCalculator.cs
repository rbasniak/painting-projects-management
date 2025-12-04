using PaintingProjectsManagement.Features.Currency;
using System.Text.RegularExpressions;

namespace PaintingProjectsManagement.Features.Projects;

public interface IProjectCostCalculator
{
    Task<ProjectCostBreakdown> CalculateCostAsync(Guid projectId, string currency, CancellationToken cancellationToken);
}

internal class ProjectCostCalculator(
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
            .First(p => p.Id == projectId);

        var projectMaterialsById = project.Materials.ToDictionary(x => x.MaterialId);

        var projectMaterials = context.Set<Material>()
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

        foreach (var categoryGroup in projectMaterials.GroupBy(x => x.Value.MaterialDefinition.CategoryId).OrderBy(x => x.Key))
        {
            var materialsCosts = new List<MaterialsCost>();

            var resinWasteFactor = categoryGroup.Key == 10 ? projectSettings.ResinWasteFactor : 1.0; // TODO: Dangerous hardcoded value (resins)

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

            costBreakdown.Materials.Add(categoryGroup.First().Value.MaterialDefinition.CategoryName, materialsCosts.AsReadOnly());
        }

        return costBreakdown;
    }
}