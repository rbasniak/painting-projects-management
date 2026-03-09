using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace PaintingProjectsManagement.Features.Authorization;

public static class Builder
{
    public static IServiceCollection AddAuthenticationFeature(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder UseAuthenticationFeature(this IEndpointRouteBuilder app)
    {
        return UseCasesBuilder.MapAuthenticationFeature(app);
    }
}
