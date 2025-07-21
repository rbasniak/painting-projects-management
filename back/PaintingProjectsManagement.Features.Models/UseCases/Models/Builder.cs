namespace PaintingProjectsManagement.Features.Models;

public static class ModelsBuilder
{
    public static IEndpointRouteBuilder MapModelsFeature(this IEndpointRouteBuilder app)
    {
        CreateModel.MapEndpoint(app);
        UpdateModel.MapEndpoint(app);
        DeleteModel.MapEndpoint(app);
        ListModels.MapEndpoint(app);
        UploadModelPicture.MapEndpoint(app); 
        PrioritizeModels.MapEndpoint(app);
        ListPriorityModels.MapEndpoint(app);

        return app;
    }
}