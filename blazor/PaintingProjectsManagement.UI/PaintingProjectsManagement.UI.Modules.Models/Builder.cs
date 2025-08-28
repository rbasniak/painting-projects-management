using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Models;

public static class Builder
{
    public static IServiceCollection AddModelsModule(this IServiceCollection services)
    {
        services.AddSingleton<IModule, ModelsModule>();

        services.AddScoped<IModelCategoriesService>(sp =>
        {
            var handler = sp.GetRequiredService<BearerDelegatingHandler>();

            // Assign the inner handler (required!!!!)
            handler.InnerHandler = new HttpClientHandler();

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:7236")
            };
            return new ModelCategoriesService(httpClient);
        });

        services.AddScoped<IModelsService>(sp =>
        {
            var handler = sp.GetRequiredService<BearerDelegatingHandler>();

            // Assign the inner handler (required!!!!)
            handler.InnerHandler = new HttpClientHandler();

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:7236")
            };
            return new ModelsService(httpClient);
        });

        return services;
    }
}

public class ModelsModule : IModule
{
    public string Name => "Models";
    public string Route => "models";
    public string Icon => "icon";
    public int Order => 2;
    public IModuleRoute[] Routes => [
        new ModuleRoute
        {
            Name = "Library",
            Route = "library",
            Icon = "icon",
            Order = 1
        },
        new ModuleRoute
        {
            Name = "Classification",
            Route = "classification",
            Icon = "icon",
            Order = 2
        },
        new ModuleRoute
        {
            Name = "Categories",
            Route = "categories",
            Icon = "icon",
            Order = 3
        }
    ];
}
