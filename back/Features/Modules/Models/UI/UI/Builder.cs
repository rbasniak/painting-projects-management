using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.UI.Modules.Models;
using PaintingProjectsManagement.UI.Modules.Shared;

namespace Microsoft.Extensions.DependencyInjection;

public static class Builder
{
    public static IServiceCollection AddModelsModule(this IServiceCollection services)
    {
        services.AddSingleton<IModule, Menu>();

        services.AddScoped<IModelCategoriesService>(sp =>
        {
            var bearer = sp.GetRequiredService<BearerDelegatingHandler>();
            var errorHandler = sp.GetRequiredService<HttpErrorHandler>();

            bearer.InnerHandler = new HttpClientHandler();
            errorHandler.InnerHandler = bearer;

            var httpClient = new HttpClient(errorHandler)
            {
                BaseAddress = new Uri("https://localhost:7236")
            };
            return new ModelCategoriesService(httpClient);
        });

        services.AddScoped<IModelsService>(sp =>
        {
            var bearer = sp.GetRequiredService<BearerDelegatingHandler>();
            var errorHandler = sp.GetRequiredService<HttpErrorHandler>();

            bearer.InnerHandler = new HttpClientHandler();
            errorHandler.InnerHandler = bearer;

            var httpClient = new HttpClient(errorHandler)
            {
                BaseAddress = new Uri("https://localhost:7236")
            };
            return new ModelsService(httpClient);
        });

        return services;
    }
}
