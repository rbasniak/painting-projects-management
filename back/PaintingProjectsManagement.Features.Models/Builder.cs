namespace PaintingProjectsManagement.Features.Models;

public static class Builder
{
    public static IEndpointRouteBuilder MapPrintingModelsFeature(this IEndpointRouteBuilder app)
    {
        app.MapModelCategoriesFeature();
        app.MapModelsFeature();

        return app;
    }
} 