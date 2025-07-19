using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace rbkApiModules.Commons.Core.UiDefinitions;

public static class CoreUiDefinitionsBuilder
{
    public static void AddRbkUIDefinitions(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddSingleton(new UIDefinitionOptions(assemblies));
    }

    public static IApplicationBuilder UseRbkUIDefinitions(this WebApplication app)
    {
        UiDefinitionsEndpoints.MapEndpoints(app);

        return app;
    }
}
