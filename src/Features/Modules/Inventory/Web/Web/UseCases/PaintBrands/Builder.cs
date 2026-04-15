namespace PaintingProjectsManagement.Features.Inventory;

public static class PaintBrandsBuilder
{
    public static IEndpointRouteBuilder MapPaintBrandsFeature(this IEndpointRouteBuilder app)
    {
        CreatePaintBrand.MapEndpoint(app);
        UpdatePaintBrand.MapEndpoint(app);
        DeletePaintBrand.MapEndpoint(app);
        ListPaintBrands.MapEndpoint(app);

        return app;
    }
}