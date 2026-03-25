using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.UI.Modules.Inventory;
using PaintingProjectsManagement.UI.Modules.Shared;

namespace Microsoft.Extensions.DependencyInjection;

public static class Builder
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, Uri apiBaseAddress)
    {
        services.AddSingleton<IModule, Menu>();

        services.AddScoped<IInventoryService>(x =>
        {
            var bearer = x.GetRequiredService<BearerDelegatingHandler>();
            var errorHandler = x.GetRequiredService<HttpErrorHandler>();

            bearer.InnerHandler = new HttpClientHandler();
            errorHandler.InnerHandler = bearer;

            var httpClient = new HttpClient(errorHandler)
            {
                BaseAddress = apiBaseAddress
            };
            return new InventoryService(httpClient);
        });

        return services;
    }
}
