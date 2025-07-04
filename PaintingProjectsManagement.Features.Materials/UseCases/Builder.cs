namespace PaintingProjectsManagement.Features.Materials;

public static class Builder
{
    public static IEndpointRouteBuilder MapMaterialsFeature(this IEndpointRouteBuilder app)
    {
        CreateMaterial.MapEndpoint(app);
        UpdateMaterial.MapEndpoint(app);
        DeleteMaterial.MapEndpoint(app);
        ListMaterials.MapEndpoint(app);

        return app;
    }
}
