using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace PaintingProjectsManagement.Features.Currency;

public static class Builder
{
    public static IServiceCollection AddCurrencyFeature(this IServiceCollection services)
    {
        services.AddCurrencyConverter();
        services.AddScoped<ICurrencyCatalog, CurrencyCatalog>();

        return services;
    }

    public static IEndpointRouteBuilder MapCurrencyFeature(this IEndpointRouteBuilder app)
    {
        return app.MapCurrencyEndpoint();
    }
}
