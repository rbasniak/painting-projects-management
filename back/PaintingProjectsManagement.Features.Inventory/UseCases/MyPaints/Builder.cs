namespace PaintingProjectsManagement.Features.Inventory;

public static class MyPaintsBuilder
{
    public static IEndpointRouteBuilder MapMyPaintsFeature(this IEndpointRouteBuilder app)
    {
        ListMyPaints.MapEndpoint(app);
        AddToMyPaints.MapEndpoint(app);
        RemoveFromMyPaints.MapEndpoint(app);
        return app;
    }
}
