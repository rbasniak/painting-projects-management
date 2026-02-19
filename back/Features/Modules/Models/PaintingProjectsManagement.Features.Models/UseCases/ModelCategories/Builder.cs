namespace PaintingProjectsManagement.Features.Models;

public static class ModelCategoriesBuilder
{
    public static IEndpointRouteBuilder MapModelCategoriesFeature(this IEndpointRouteBuilder app)
    {
        CreateModelCategory.MapEndpoint(app);
        UpdateModelCategory.MapEndpoint(app);
        DeleteModelCategory.MapEndpoint(app);
        ListModelCategories.MapEndpoint(app);

        return app;
    }
}