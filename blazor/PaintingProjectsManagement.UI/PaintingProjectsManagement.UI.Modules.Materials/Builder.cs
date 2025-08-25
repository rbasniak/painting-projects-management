using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagement.Blazor.Modules.Authentication;

namespace PaintingProjectsManagement.UI.Modules.Materials;

public static class Builder
{
    public static IServiceCollection AddMaterialsModule(this IServiceCollection services)
    {
        services.AddScoped<IMaterialsService>(sp =>
        {
            var handler = sp.GetRequiredService<BearerDelegatingHandler>();

            // Assign the inner handler (required!!!!)
            handler.InnerHandler = new HttpClientHandler();

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:7236")
            };
            return new MaterialsService(httpClient);
        });

        return services;
    }
} 