using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.UI.Modules.Materials;
using PaintingProjectsManagement.UI.Modules.Shared;

namespace Microsoft.Extensions.DependencyInjection;

public static class Builder
{
    public static IServiceCollection AddMaterialsModule(this IServiceCollection services, Uri apiBaseAddress)
    {
        services.AddSingleton<IModule, Menu>();

        services.AddScoped<IMaterialsService>(x =>
        {
            var bearer = x.GetRequiredService<BearerDelegatingHandler>();
            var errorHandler = x.GetRequiredService<HttpErrorHandler>();

            bearer.InnerHandler = new HttpClientHandler();
            errorHandler.InnerHandler = bearer;

            var httpClient = new HttpClient(errorHandler)
            {
                BaseAddress = apiBaseAddress
            };
            return new MaterialsService(httpClient);
        });

        return services;
    }
}
