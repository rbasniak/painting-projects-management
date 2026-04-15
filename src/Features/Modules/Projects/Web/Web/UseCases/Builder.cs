namespace PaintingProjectsManagement.Features.Projects;

public static class UseCasesBuilder
{
    internal static IEndpointRouteBuilder MapProjectsFeature(IEndpointRouteBuilder app)
    {
        return ProjectsEndpointsMap.MapProjectsFeature(app);
    }
}
