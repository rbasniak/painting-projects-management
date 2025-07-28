namespace PaintingProjectsManagement.Features.Models;

public static class ModelsBuilder
{
    public static IEndpointRouteBuilder MapModelsFeature(this IEndpointRouteBuilder app)
    {
        RateModel.MapEndpoint(app);
        CreateModel.MapEndpoint(app);
        UpdateModel.MapEndpoint(app);
        DeleteModel.MapEndpoint(app);
        ListModels.MapEndpoint(app);
        UploadModelPicture.MapEndpoint(app); 
        SetModelMustHave.MapEndpoint(app);
        ListPriorityModels.MapEndpoint(app);

        return app;
    }
}