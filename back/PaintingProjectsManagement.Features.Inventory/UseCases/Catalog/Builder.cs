namespace PaintingProjectsManagement.Features.Inventory;

public static class CatalogBuilder
{
    public static IEndpointRouteBuilder MapCatalogFeature(this IEndpointRouteBuilder app)
    {
        GetCatalog.MapEndpoint(app);
        return app;
    }
}
