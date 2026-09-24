namespace PaintingProjectsManagement.Features.Models;

public static class UseCasesBuilder
{
    internal static IEndpointRouteBuilder MapPrintingModelsFeature(IEndpointRouteBuilder app)
    {
        app.MapModelCategoriesFeature();
        app.MapModelsFeature();

        return app;
    }
}
