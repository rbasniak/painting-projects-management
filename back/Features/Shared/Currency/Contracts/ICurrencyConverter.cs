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
        if (string.IsNullOrWhiteSpace(fromCurrency))
        {
            throw new ArgumentException("From currency is required.", nameof(fromCurrency));
        }

        if (string.IsNullOrWhiteSpace(toCurrency))
        {
            throw new ArgumentException("To currency is required.", nameof(toCurrency));
        }

        var from = fromCurrency.Trim().ToUpperInvariant();
        var to = toCurrency.Trim().ToUpperInvariant();

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
                var response = await _httpClient.GetAsync($"/latest?from={from}");
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // #region agent log
                    try
                    {
                        var snippet = content.Length > 240 ? content[..240] : content;
                        System.IO.File.AppendAllText(
                            "/opt/cursor/logs/debug.log",
                            JsonSerializer.Serialize(new
                            {
                                hypothesisId = "H2",
                                location = "ICurrencyConverter.cs:GetConversionRate",
                                message = "Frankfurter non-success",
                                data = new { from, to, status = (int)response.StatusCode, bodySnippet = snippet },
                                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            }) + "\n");
                    }
                    catch
                    {
                        /* ignore debug log I/O errors */
                    }
                    // #endregion

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

        // #region agent log
        try
        {
            System.IO.File.AppendAllText(
                "/opt/cursor/logs/debug.log",
                JsonSerializer.Serialize(new
                {
                    hypothesisId = "H2b",
                    location = "ICurrencyConverter.cs:GetConversionRate",
                    message = "Target currency missing in rates table",
                    data = new { from, to, rateKeyCount = ratesTable.Count },
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }) + "\n");
        }
        catch
        {
            /* ignore */
        }
        // #endregion

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
