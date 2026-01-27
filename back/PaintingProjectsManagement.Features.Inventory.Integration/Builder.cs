using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagement.Features.Inventory.Integration;

namespace PaintingProjectsManagement.Features.Inventory.Integration;

public static class Builder
{
    public static IServiceCollection AddInventoryFeature(this IServiceCollection services)
    {
        services.AddScoped<IRequestHandler<FindColorMatchesCommandRequest, QueryResponse<IReadOnlyCollection<ColorMatchResult>>>, FindColorMatches.Handler>();

        return services;
    }

    public static IEndpointRouteBuilder MapInventoryFeature(this IEndpointRouteBuilder app)
    {
        return app;
    }
}