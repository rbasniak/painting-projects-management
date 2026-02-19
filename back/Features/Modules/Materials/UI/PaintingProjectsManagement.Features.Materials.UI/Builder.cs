using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.UI.Modules.Materials;
using PaintingProjectsManagement.UI.Modules.Shared;

namespace Microsoft.Extensions.DependencyInjection;

public static class Builder
{
    public static IServiceCollection AddMaterialsModule(this IServiceCollection services)
    {
        services.AddSingleton<IModule, Menu>();

        services.AddScoped<IMaterialsService>(sp =>
        {
            var bearer = sp.GetRequiredService<BearerDelegatingHandler>();
            var errorHandler = sp.GetRequiredService<HttpErrorHandler>();

            bearer.InnerHandler = new HttpClientHandler();
            errorHandler.InnerHandler = bearer;

            var httpClient = new HttpClient(errorHandler)
            {
                BaseAddress = new Uri("https://localhost:7236")
            };
            return new MaterialsService(httpClient);
        });

        return services;
    }
}
