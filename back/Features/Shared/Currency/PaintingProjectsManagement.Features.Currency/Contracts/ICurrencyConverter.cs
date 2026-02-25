using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace PaintingProjectsManagement.Features.Currency;

public interface ICurrencyConverter
{
    Task<double> GetConversionRate(string fromCurrency, string toCurrency);
    Task<Dictionary<string, string>> GetAvailableCurrencies();
}

internal class CurrencyConverter : ICurrencyConverter
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string ApiBaseUrl = "https://api.frankfurter.app";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public CurrencyConverter(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
        _httpClient.BaseAddress = new Uri(ApiBaseUrl);
    }

    public async Task<double> GetConversionRate(string fromCurrency, string toCurrency)
    {
        if (fromCurrency.Equals(toCurrency, StringComparison.InvariantCultureIgnoreCase))
        {
            return 1.0;
        }

        var cacheKey = $"conversion_rate_{fromCurrency.ToUpper()}_{toCurrency.ToUpper()}";
        
        if (_cache.TryGetValue(cacheKey, out double cachedRate))
        {
            return cachedRate;
        }

        try
        {
            var response = await _httpClient.GetAsync($"/latest?from={fromCurrency.ToUpper()}&to={toCurrency.ToUpper()}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FrankfurterResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Rates != null && result.Rates.TryGetValue(toCurrency.ToUpper(), out var rate))
            {
                _cache.Set(cacheKey, rate, CacheDuration);
                return rate;
            }

            throw new InvalidOperationException($"Unable to get conversion rate from {fromCurrency} to {toCurrency}");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to fetch currency conversion rate: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<string, string>> GetAvailableCurrencies()
    {
        const string cacheKey = "available_currencies";
        
        if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? cachedCurrencies))
        {
            return cachedCurrencies!;
        }

        try
        {
            var response = await _httpClient.GetAsync("/currencies");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var currencies = JsonSerializer.Deserialize<Dictionary<string, string>>(content) ?? new Dictionary<string, string>();

            _cache.Set(cacheKey, currencies, CacheDuration);
            return currencies;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to fetch available currencies: {ex.Message}", ex);
        }
    }

    private class FrankfurterResponse
    {
        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("base")]
        public string Base { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("rates")]
        public Dictionary<string, double>? Rates { get; set; }
    }
}
