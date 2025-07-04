using Microsoft.AspNetCore.Builder;

namespace PaintingProjectsManagement.Features.Projects;

public static class Builder
{
    public static IEndpointRouteBuilder MapProjectsFeature(this IEndpointRouteBuilder app)
    {
        ListProjects.MapEndpoint(app);
        GetProjectDetails.MapEndpoint(app);
        
        CreateProject.MapEndpoint(app);
        UpdateProject.MapEndpoint(app);
        DeleteProject.MapEndpoint(app);

        return app;
    }
}