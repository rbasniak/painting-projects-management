namespace PaintingProjectsManagement.Features.Subscriptions;

public static class UseCasesBuilder
{
    internal static IEndpointRouteBuilder MapSubscriptionsFeature(IEndpointRouteBuilder app)
    {
        GetCurrentSubscription.MapEndpoint(app);
        ListSubscriptionTiers.MapEndpoint(app);
        Subscribe.MapEndpoint(app);
        UpgradeSubscription.MapEndpoint(app);
        CancelSubscription.MapEndpoint(app);
        return app;
    }
}
