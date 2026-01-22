namespace PaintingProjectsManagement.Features.Inventory;

public static class Builder
{
    public static IEndpointRouteBuilder MapInventoryFeature(this IEndpointRouteBuilder app)
    {
        app.MapCatalogFeature();
        app.MapMyPaintsFeature();
        app.MapPaintBrandsFeature();
        app.MapPaintLinesFeature();
        app.MapPaintColorsFeature();

        return app;
    }
}