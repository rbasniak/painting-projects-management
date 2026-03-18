using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagement.Features.Subscriptions.Integration;

namespace PaintingProjectsManagement.Features.Subscriptions.Integration;

public static class Builder
{
    public static IServiceCollection AddSubscriptionsIntegrationHandlers(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetSubscriptionEntitlementQuery, SubscriptionEntitlementResult>, GetSubscriptionEntitlement.Handler>();
        return services;
    }

    public static IEndpointRouteBuilder MapSubscriptionsIntegrationFeature(this IEndpointRouteBuilder app)
    {
        return app;
    }
}
