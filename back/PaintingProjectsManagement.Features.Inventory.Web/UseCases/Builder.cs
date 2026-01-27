using Microsoft.Extensions.DependencyInjection;

namespace PaintingProjectsManagement.Features.Inventory;

public static class Builder
{
    public static IServiceCollection AddInventoryFeature(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapInventoryFeature(this IEndpointRouteBuilder app)
    {
        app.MapCatalogueFeature();
        app.MapMyPaintsFeature();
        app.MapPaintBrandsFeature();
        app.MapPaintLinesFeature();
        app.MapPaintColorsFeature();

        return app;
    }
}