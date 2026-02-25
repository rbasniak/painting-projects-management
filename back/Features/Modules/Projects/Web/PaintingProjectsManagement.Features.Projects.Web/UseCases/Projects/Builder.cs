namespace PaintingProjectsManagement.Features.Projects;

internal static class ProjectsEndpointsMap
{
    public static IEndpointRouteBuilder MapProjectsFeature(this IEndpointRouteBuilder app)
    {
        ListProjects.MapEndpoint(app);
        GetProjectDetails.MapEndpoint(app);
        
        CreateProject.MapEndpoint(app);
        UpdateProject.MapEndpoint(app);
        DeleteProject.MapEndpoint(app);
        AddProjectMaterial.MapEndpoint(app);
        UpdateProjectMaterial.MapEndpoint(app);
        DeleteProjectMaterial.MapEndpoint(app);
        AddProjectStepSpan.MapEndpoint(app);
        AddProjectStep.MapEndpoint(app);
        UpdateProjectStep.MapEndpoint(app);
        DeleteProjectStep.MapEndpoint(app);
        GetProjectMaterials.MapEndpoint(app);
        GetProjectSteps.MapEndpoint(app);

        CreateColorGroup.MapEndpoint(app);
        UpdateColorGroup.MapEndpoint(app);
        DeleteColorGroup.MapEndpoint(app);
        UpdateReferenceColor.MapEndpoint(app);
        MatchPaints.MapEndpoint(app);
        UpdatePickedColor.MapEndpoint(app);

        UploadProjectReferencePicture.MapEndpoint(app);
        DeleteProjectReferencePicture.MapEndpoint(app);

        return app;
    }
}
