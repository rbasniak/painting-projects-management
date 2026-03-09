namespace PaintingProjectsManagement.Features.Models.Integrations.Api;

public static class UseCasesBuilder
{
    internal static IEndpointRouteBuilder MapModelsIntegrationsApi(IEndpointRouteBuilder app)
    {
        app.MapModelsIntegrationsEndpoints();
        return app;
    }
}
