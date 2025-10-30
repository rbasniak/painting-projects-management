using Microsoft.Extensions.DependencyInjection;

namespace PaintingProjectsManagement.Features.Currency;

public static class CurrencyServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyConverter(this IServiceCollection services)
    {
        services.AddHttpClient<ICurrencyConverter, CurrencyConverter>();
        
     return services;
    }
}
