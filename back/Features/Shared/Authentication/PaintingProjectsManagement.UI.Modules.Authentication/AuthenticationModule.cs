using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.UI.Modules.Shared;

namespace Microsoft.Extensions.DependencyInjection;

public static class Builder
{
    public static IServiceCollection AddAuthenticationModule(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddTransient<BearerDelegatingHandler>();

        services.AddScoped<IAuthenticationService>(sp =>
        {
            var errorHandler = sp.GetRequiredService<HttpErrorHandler>();
            errorHandler.InnerHandler = new HttpClientHandler();

            var httpClient = new HttpClient(errorHandler)
            {
                BaseAddress = new Uri("https://localhost:7236")
            };
            return new AuthenticationService(httpClient);
        });

        return services;
    }
}