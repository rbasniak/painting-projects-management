using Microsoft.Extensions.DependencyInjection;

namespace PaintingProjectsManagement.Features.Materials.Integration;

public static class Builder
{
    public static IServiceCollection AddMaterialsIntegrations(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventHandler<MaterialCreated>, MaterialCreatedHandler>();
        services.AddScoped<IDomainEventHandler<MaterialDeleted>, MaterialDeletedHandler>();
        services.AddScoped<IDomainEventHandler<MaterialPackageContentChanged>, MaterialUpdatedHandler>();
        services.AddScoped<IDomainEventHandler<MaterialPackagePriceChanged>, MaterialUpdatedHandler>();
        services.AddScoped<IDomainEventHandler<MaterialNameChanged>, MaterialUpdatedHandler>();
        services.AddScoped<IDomainEventHandler<MaterialCategoryChanged>, MaterialUpdatedHandler>();

        return services;
    }
}
