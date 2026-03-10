namespace PaintingProjectsManagement.Features.Authorization;

public static class ProfileBuilder
{
    public static IEndpointRouteBuilder MapProfileFeature(this IEndpointRouteBuilder app)
    {
        GetProfile.MapEndpoint(app);
        GetStorageUsage.MapEndpoint(app);
        return app;
    }
}
