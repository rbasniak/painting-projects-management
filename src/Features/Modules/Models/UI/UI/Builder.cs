using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.UI.Modules.Models;
using PaintingProjectsManagement.UI.Modules.Shared;

namespace Microsoft.Extensions.DependencyInjection;

public static class Builder
{
    public static IServiceCollection AddModelsModule(this IServiceCollection services, Uri apiBaseAddress)
    {
        services.AddSingleton<IModule, Menu>();

        services.AddScoped<IModelCategoriesService>(x =>
        {
            var bearer = x.GetRequiredService<BearerDelegatingHandler>();
            var errorHandler = x.GetRequiredService<HttpErrorHandler>();

            bearer.InnerHandler = new HttpClientHandler();
            errorHandler.InnerHandler = bearer;

            var httpClient = new HttpClient(errorHandler)
            {
                BaseAddress = apiBaseAddress
            };
            return new ModelCategoriesService(httpClient);
        });

        services.AddScoped<IModelsService>(x =>
        {
            var bearer = x.GetRequiredService<BearerDelegatingHandler>();
            var errorHandler = x.GetRequiredService<HttpErrorHandler>();

            bearer.InnerHandler = new HttpClientHandler();
            errorHandler.InnerHandler = bearer;

            var httpClient = new HttpClient(errorHandler)
            {
                BaseAddress = apiBaseAddress
            };
            return new ModelsService(httpClient);
        });

        return services;
    }
}
