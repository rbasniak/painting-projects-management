using PaintingProjectsManagement.Features.Projects;

namespace Microsoft.Extensions.DependencyInjection;

public static class Builder
{
    public static IServiceCollection AddProjectsFeature(this IServiceCollection services)
    {
        services.AddScoped<ProjectCostCalculator>();
        services.AddTransient<IUnitsConverter>();
        services.AddApplicationOptions<ProjectSettings>();
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