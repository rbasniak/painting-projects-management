namespace PaintingProjectsManagement.Features.Authorization;

public static class UseCasesBuilder
{
    internal static IEndpointRouteBuilder MapAuthenticationFeature(IEndpointRouteBuilder app)
    {
        app.MapProfileFeature();
        return app;
    }
}
