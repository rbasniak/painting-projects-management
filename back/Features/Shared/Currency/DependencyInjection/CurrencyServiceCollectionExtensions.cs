
using PaintingProjectsManagement.Features.Currency;

namespace Microsoft.Extensions.DependencyInjection;

public static class CurrencyServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyConverter(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpClient<ICurrencyConverter, CurrencyConverter>()
            .ConfigureHttpClient(static client => client.Timeout = TimeSpan.FromSeconds(20));
        
        return services;
    }
}
