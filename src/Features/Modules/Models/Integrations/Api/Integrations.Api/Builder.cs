using Microsoft.Extensions.DependencyInjection;

namespace PaintingProjectsManagement.Features.Models.Integrations.Api;

public static class Builder
{
    public static IServiceCollection AddModelsIntegrationsApi(this IServiceCollection services)
    {
        services
            .AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, ModelsIntegrationsApiKeyAuthenticationHandler>(
                ModelsIntegrationsApiAuthentication.SchemeName,
                _ => { });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(ModelsIntegrationsApiAuthentication.PolicyName, policy =>
            {
                policy
                    .AddAuthenticationSchemes(ModelsIntegrationsApiAuthentication.SchemeName)
                    .RequireAuthenticatedUser();
            });
        });

        return services;
    }

    public static IEndpointRouteBuilder MapModelsIntegrationsApi(this IEndpointRouteBuilder app)
    {
        return UseCasesBuilder.MapModelsIntegrationsApi(app);
    }
}
