namespace PaintingProjectsManagement.Features.Paints;

public static class Builder
{
    public static IEndpointRouteBuilder MapPaintsFeature(this IEndpointRouteBuilder app)
    {
        app.MapPaintBrandsFeature();
        app.MapPaintLinesFeature();
        app.MapPaintColorsFeature();

        return app;
    }
}