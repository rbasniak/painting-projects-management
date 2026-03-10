using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace PaintingProjectsManagement.Features.Currency;

public interface ICurrencyCatalog
{
    Task<IReadOnlyCollection<CurrencyOption>> ListAvailableCurrenciesAsync(CancellationToken cancellationToken);
}

public sealed record CurrencyOption(string Code, string Name);

internal sealed class CurrencyCatalog(
    ICurrencyConverter currencyConverter,
    IMemoryCache cache,
    ILogger<CurrencyCatalog> logger) : ICurrencyCatalog
{
    private const string CacheKey = "currency_catalog";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);

    public async Task<IReadOnlyCollection<CurrencyOption>> ListAvailableCurrenciesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (cache.TryGetValue(CacheKey, out IReadOnlyCollection<CurrencyOption>? cachedCurrencies) && cachedCurrencies is not null)
        {
            return cachedCurrencies;
        }

        try
        {
            var currencies = await currencyConverter.GetAvailableCurrencies();

            var options = currencies
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .Select(x => new CurrencyOption(x.Key.ToUpperInvariant(), x.Value))
                .OrderBy(x => x.Code, StringComparer.Ordinal)
                .ToArray();

            cache.Set(CacheKey, options, CacheDuration);

            return options;
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Could not load currencies from the external provider.");
            throw;
        }
    }
}
