namespace PaintingProjectsManagement.Features.Models.Integrations.Api;

public static class ModelsBuilder
{
    public static IEndpointRouteBuilder MapModelsIntegrationsEndpoints(this IEndpointRouteBuilder app)
    {
        UpsertModel.MapEndpoint(app);
        DeleteModel.MapEndpoint(app);
        UploadModelPicture.MapEndpoint(app);
        ListModels.MapEndpoint(app);

        return app;
    }
}
