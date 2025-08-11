using PaintingProjectsManagement.Blazor.Modules.Materials;
using PaintingProjectsManagement.Blazor.Modules.Shared;

namespace PaintingProjectsManagement.Blazor.Modules.Authentication;

public static class Builder
{
    public static IServiceCollection AddAuthenticationModule(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddTransient<BearerDelegatingHandler>();

        services.AddScoped<IAuthenticationService>(sp =>
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://localhost:7236")
            };
            return new AuthenticationService(httpClient);
        });

        return services;
    }
}