using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.UI.Modules.Shared;
using PaintingProjectsManagement.UI.Modules.Subscriptions;

namespace Microsoft.Extensions.DependencyInjection;

public static class Builder
{
    public static IServiceCollection AddSubscriptionsModule(this IServiceCollection services, Uri apiBaseAddress)
    {
        services.AddSingleton<IModule, Menu>();

        services.AddScoped<ISubscriptionsService>(x =>
        {
            var bearer = x.GetRequiredService<BearerDelegatingHandler>();
            var errorHandler = x.GetRequiredService<HttpErrorHandler>();

            bearer.InnerHandler = new HttpClientHandler();
            errorHandler.InnerHandler = bearer;

            var httpClient = new HttpClient(errorHandler)
            {
                BaseAddress = apiBaseAddress
            };
            return new SubscriptionsService(httpClient);
        });

        return services;
    }
}
