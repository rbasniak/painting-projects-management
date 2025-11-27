
using PaintingProjectsManagement.Features.Materials;

namespace Microsoft.Extensions.DependencyInjection;

public static class Builder
{
    public static IServiceCollection AddMaterialsFeature(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventHandler<MaterialCreated>, MaterialCreatedHandler>();
        services.AddScoped<IDomainEventHandler<MaterialDeleted>, MaterialDeletedHandler>();
        services.AddScoped<IDomainEventHandler<MaterialPackageContentChanged>, MaterialUpdatedHandler>();
        services.AddScoped<IDomainEventHandler<MaterialPackagePriceChanged>, MaterialUpdatedHandler>();
        services.AddScoped<IDomainEventHandler<MaterialNameChanged>, MaterialUpdatedHandler>();

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
