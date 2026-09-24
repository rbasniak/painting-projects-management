using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace PaintingProjectsManagement.Features.Materials;

public static class Builder
{
    public static IServiceCollection AddMaterialsFeature(this IServiceCollection services)
    {
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
