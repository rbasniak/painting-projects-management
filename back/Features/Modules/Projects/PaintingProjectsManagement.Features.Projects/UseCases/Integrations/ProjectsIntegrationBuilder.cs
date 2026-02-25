using PaintingProjectsManagement.Features.Materials.Abstractions;
using PaintingProjectsManagement.Features.Projects;

namespace Microsoft.Extensions.DependencyInjection;

public static class ProjectsIntegrationBuilder
{
    public static IServiceCollection AddProjectsIntegrationHandlers(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventHandler<MaterialCreatedV1>, MaterialCreatedConsumer>();
        services.AddScoped<IIntegrationEventHandler<MaterialUpdatedV1>, MaterialUpdatedConsumer>();
        services.AddScoped<IIntegrationEventHandler<MaterialDeletedV1>, MaterialDeletedConsumer>();
        services.AddHostedService<ProjectsIntegrationConsumer>();
        return services;
    }
}
