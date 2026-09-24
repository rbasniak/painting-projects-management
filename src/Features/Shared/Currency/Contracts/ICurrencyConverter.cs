using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;

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
    private readonly ILogger<CurrencyConverter> _logger;
    private const string ApiBaseUrl = "https://api.frankfurter.app";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(750)
    ];

    public CurrencyConverter(HttpClient httpClient, IMemoryCache cache, ILogger<CurrencyConverter> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(ApiBaseUrl);
    }

    public async Task<double> GetConversionRate(string fromCurrency, string toCurrency)
    {
        var from = CurrencyCode.Normalize(fromCurrency);
        var to = CurrencyCode.Normalize(toCurrency);

        if (string.IsNullOrWhiteSpace(from))
        {
            throw new ArgumentException("From currency is required.", nameof(fromCurrency));
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            throw new ArgumentException("To currency is required.", nameof(toCurrency));
        }

        if (string.Equals(from, to, StringComparison.Ordinal))
        {
            return 1.0;
        }

        // Cache the full ECB rate table for the base currency (one HTTP call per distinct "from"
        // per hour). Resolves target via case-insensitive lookup instead of relying on Frankfurter's
        // optional `to=` filter, which can behave inconsistently for some pairs or API versions.
        var tableCacheKey = $"latest_rates_{from}";

        if (!_cache.TryGetValue(tableCacheKey, out Dictionary<string, double>? ratesTable) || ratesTable is null)
        {
            try
            {
                var (response, content) = await GetLatestRatesAsync(from);

                if (!response.IsSuccessStatusCode)
                {
                    var snippet = content.Length > 240 ? content[..240] : content;
                    _logger.LogWarning(
                        "Frankfurter non-success when loading rates for base {From} (target {To}): HTTP {Status} body: {BodySnippet}",
                        from,
                        to,
                        (int)response.StatusCode,
                        snippet);

                    throw new InvalidOperationException(
                        $"Frankfurter HTTP {(int)response.StatusCode} when loading rates for base {from}.");
                }

                var result = JsonSerializer.Deserialize<FrankfurterResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Rates is null || result.Rates.Count == 0)
                {
                    throw new InvalidOperationException($"Frankfurter returned no rates for base currency {from}.");
                }

                ratesTable = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                foreach (var kv in result.Rates)
                {
                    ratesTable[kv.Key] = kv.Value;
                }

                _cache.Set(tableCacheKey, ratesTable, CacheDuration);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to fetch currency conversion rate: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse Frankfurter currency response.", ex);
            }
        }

        if (ratesTable.TryGetValue(to, out var rate))
        {
            return rate;
        }

        _logger.LogWarning(
            "Target currency {To} missing in Frankfurter rate table for base {From} ({RateCount} keys).",
            to,
            from,
            ratesTable.Count);

        throw new InvalidOperationException($"Unable to get conversion rate from {from} to {to}.");
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

    private async Task<(HttpResponseMessage Response, string Content)> GetLatestRatesAsync(string from)
    {
        HttpResponseMessage? lastResponse = null;
        string lastContent = string.Empty;

        for (var attempt = 0; attempt <= RetryDelays.Length; attempt++)
        {
            lastResponse?.Dispose();
            lastResponse = await _httpClient.GetAsync($"/latest?from={from}");
            lastContent = await lastResponse.Content.ReadAsStringAsync();

            if (lastResponse.IsSuccessStatusCode || !ShouldRetry(lastResponse.StatusCode) || attempt == RetryDelays.Length)
            {
                return (lastResponse, lastContent);
            }

            _logger.LogWarning(
                "Retrying Frankfurter latest rates request for base {From} after transient HTTP {Status}. Attempt {Attempt} of {TotalAttempts}.",
                from,
                (int)lastResponse.StatusCode,
                attempt + 1,
                RetryDelays.Length + 1);

            await Task.Delay(RetryDelays[attempt]);
        }

        return (lastResponse!, lastContent);
    }

    private static bool ShouldRetry(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.TooManyRequests
            or HttpStatusCode.BadGateway
            or HttpStatusCode.GatewayTimeout
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.Forbidden;
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
