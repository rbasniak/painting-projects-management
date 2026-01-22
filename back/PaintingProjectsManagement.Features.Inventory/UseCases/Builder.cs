using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagement.Features.Inventory.Integration;

namespace PaintingProjectsManagement.Features.Inventory;

public static class Builder
{
    public static IServiceCollection AddInventoryFeature(this IServiceCollection services)
    {
        services.AddScoped<IRequestHandler<FindColorMatchesCommandRequest, QueryResponse<IReadOnlyCollection<ColorMatchResult>>>, FindColorMatches.Handler>();

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