using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagement.Features.Subscriptions.Integration;

namespace PaintingProjectsManagement.Features.Subscriptions;

public static class Builder
{
    public static IServiceCollection AddSubscriptionsFeature(this IServiceCollection services)
    {
        services.AddSingleton<ISubscriptionTierPolicyCatalog, SubscriptionTierPolicyCatalog>();
        services.AddSingleton<IPaymentGateway, DummyPaymentGateway>();
        services.AddScoped<ISubscriptionAccessService, SubscriptionAccessService>();
        services.AddSubscriptionsIntegrationHandlers();
        services.AddHostedService<SubscriptionExpirationReconciliationService>();
        return services;
    }

    public static IEndpointRouteBuilder MapSubscriptionsFeature(this IEndpointRouteBuilder app)
    {
        return UseCasesBuilder.MapSubscriptionsFeature(app);
    }
}
