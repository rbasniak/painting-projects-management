using Microsoft.Extensions.DependencyInjection;

namespace PaintingProjectsManagement.Features.Materials;

public static class Builder
{
    public static IServiceCollection AddMaterialsFeature(this IServiceCollection services)
    {
        services.AddScoped<IEventHandler<MaterialCreated>, MaterialCreatedHandler>();
        services.AddScoped<IEventHandler<MaterialDeleted>, MaterialDeletedHandler>();
        services.AddScoped<IEventHandler<MaterialPackageContentChanged>, MaterialUpdatedHandler>();
        services.AddScoped<IEventHandler<MaterialPackagePriceChanged>, MaterialUpdatedHandler>();
        services.AddScoped<IEventHandler<MaterialNameChanged>, MaterialUpdatedHandler>();

        return services;
    }

    public static IEndpointRouteBuilder UseMaterialsFeature(this IEndpointRouteBuilder app)
    {
        CreateMaterial.MapEndpoint(app);
        UpdateMaterial.MapEndpoint(app);
        DeleteMaterial.MapEndpoint(app);
        ListMaterials.MapEndpoint(app);

        return app;
    }
}
