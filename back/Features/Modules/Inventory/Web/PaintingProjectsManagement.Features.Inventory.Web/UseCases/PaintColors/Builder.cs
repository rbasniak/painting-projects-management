namespace PaintingProjectsManagement.Features.Inventory;

public static class PaintColorsBuilder
{
    public static IEndpointRouteBuilder MapPaintColorsFeature(this IEndpointRouteBuilder app)
    {
        CreatePaintColor.MapEndpoint(app);
        UpdatePaintColor.MapEndpoint(app);
        DeletePaintColor.MapEndpoint(app);
        ListPaintColors.MapEndpoint(app);

        return app;
    }
}