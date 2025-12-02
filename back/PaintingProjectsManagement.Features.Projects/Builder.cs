using PaintingProjectsManagement.Features.Projects;

namespace Microsoft.Extensions.DependencyInjection;

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
            PrinterConsumptioninKwh = 0.18
        });
        
        services.AddProjectsIntegrationHandlers();

        return services;
    }

    public static IEndpointRouteBuilder MapProjectsFeature(this IEndpointRouteBuilder app)
    {
        ListProjects.MapEndpoint(app);
        GetProjectDetails.MapEndpoint(app);
        
        CreateProject.MapEndpoint(app);
        UpdateProject.MapEndpoint(app);
        DeleteProject.MapEndpoint(app);
        AddProjectMaterial.MapEndpoint(app);
        AddProjectStepSpan.MapEndpoint(app);

        return app;
    }
}