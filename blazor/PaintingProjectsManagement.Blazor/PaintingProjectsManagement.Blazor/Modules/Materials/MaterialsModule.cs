using Microsoft.AspNetCore.Components;
using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.Blazor.Modules.Shared;

namespace PaintingProjectsManagement.Blazor.Modules.Materials;

public class MaterialsModule : IModule
{
    public string Name => "Materials";
    public string Route => "materials";
    public string Icon => "icon";
    public int Order => 1;
}

public static class Builder
{
    public static IServiceCollection AddMaterialsModule(this IServiceCollection services)
    {
        services.AddScoped<MaterialsAdaptor>();

        services.AddScoped<IMaterialsService>(sp =>
        {
            var handler = sp.GetRequiredService<BearerDelegatingHandler>();

            // Assign the inner handler (required)
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