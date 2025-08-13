using Microsoft.Extensions.DependencyInjection;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials;

public static class MaterialsIntegrationBuilder
{
    public static IServiceCollection AddMaterialsIntegrationHandlers(this IServiceCollection services)
    {
        services.AddScoped<IEventHandler<MaterialCreated>, MaterialCreatedHandler>();
        services.AddScoped<IEventHandler<MaterialDeleted>, MaterialDeletedHandler>();
        services.AddScoped<IEventHandler<MaterialPackageContentChanged>, MaterialUpdatedHandler>();
        services.AddScoped<IEventHandler<MaterialPackagePriceChanged>, MaterialUpdatedHandler>();
        services.AddScoped<IEventHandler<MaterialNameChanged>, MaterialUpdatedHandler>();

        return services;
    }
}

