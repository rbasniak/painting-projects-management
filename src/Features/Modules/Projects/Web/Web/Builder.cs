using Microsoft.Extensions.DependencyInjection;

namespace PaintingProjectsManagement.Features.Projects;

public static class Builder
{
    public static IServiceCollection AddProjectsFeature(this IServiceCollection services)
    {
        services.AddScoped<IProjectCostCalculator, ProjectCostCalculator>();
        services.AddApplicationOptions<ProjectSettings>();
        services.AddTransient<IUnitsConverter, UnitsConverter>();

        // TODO: Get from user settings
        services.AddSingleton(new ProjectSettings
        {
            ElectricityCostPerKwh = new Money(2, "DKK"),
            LaborCostPerHour = new Money(150, "DKK"),
            MililiterPerDrop = 0.05,
            PrinterConsumptioninKwh = 0.18,
            MililiterPerSpray = 0.5,
            ResinWasteFactor = 1.55,
            MaterialMarkup = 1.0
        });
        
        services.AddProjectsIntegrationHandlers();

        return services;
    }

    public static IEndpointRouteBuilder MapProjectsFeature(this IEndpointRouteBuilder app)
    {
        return UseCasesBuilder.MapProjectsFeature((IEndpointRouteBuilder)app);
    }
}
