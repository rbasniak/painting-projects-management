using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Projects;

public static class ProjectsIntegrationBuilder
{
    public static IServiceCollection AddProjectsIntegrationHandlers(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventHandler<MaterialCreatedV1>, MaterialCreatedConsumer>();
        services.AddScoped<IIntegrationEventHandler<MaterialPackageContentChanged>, MaterialUpdatedConsumer>();
        services.AddScoped<IIntegrationEventHandler<MaterialDeletedV1>, MaterialDeletedConsumer>();
        return services;
    }
}
