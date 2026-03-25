using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.UI.Modules.Shared;

namespace Microsoft.Extensions.DependencyInjection;

public static class Builder
{
    public static IServiceCollection AddAuthenticationModule(this IServiceCollection services, Uri apiBaseAddress)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddTransient<BearerDelegatingHandler>();

        services.AddScoped<IAuthenticationService>(x =>
        {
            var errorHandler = x.GetRequiredService<HttpErrorHandler>();
            errorHandler.InnerHandler = new HttpClientHandler();

            var httpClient = new HttpClient(errorHandler)
            {
                BaseAddress = apiBaseAddress
            };
            return new AuthenticationService(httpClient);
        });

        services.AddScoped<IUserProfileService>(x =>
        {
            var bearer = x.GetRequiredService<BearerDelegatingHandler>();
            var errorHandler = x.GetRequiredService<HttpErrorHandler>();

            bearer.InnerHandler = new HttpClientHandler();
            errorHandler.InnerHandler = bearer;

            var httpClient = new HttpClient(errorHandler)
            {
                BaseAddress = apiBaseAddress
            };
            return new UserProfileService(httpClient);
        });

        return services;
    }
}