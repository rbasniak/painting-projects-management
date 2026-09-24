namespace PaintingProjectsManagement.Features.Inventory;

public static class PaintLinesBuilder
{
    public static IEndpointRouteBuilder MapPaintLinesFeature(this IEndpointRouteBuilder app)
    {
        CreatePaintLine.MapEndpoint(app);
        UpdatePaintLine.MapEndpoint(app);
        DeletePaintLine.MapEndpoint(app);
        ListPaintLines.MapEndpoint(app);

        return app;
    }
}