using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.Features.Currency;

public interface ICurrencyConverter
{
    Task<double> GetConversionRate(string fromCurrency, string toCurrency);
    Task<Dictionary<string, string>> GetAvailableCurrencies();
}

internal class CurrencyConverter : ICurrencyConverter
{
    private readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "https://api.frankfurter.app";

    public CurrencyConverter(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(ApiBaseUrl);
    }

    public async Task<double> GetConversionRate(string fromCurrency, string toCurrency)
    {
        if (fromCurrency.Equals(toCurrency, StringComparison.InvariantCultureIgnoreCase))
        {
            return 1.0;
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
        try
        {
            var response = await _httpClient.GetAsync("/currencies");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var currencies = JsonSerializer.Deserialize<Dictionary<string, string>>(content);

            return currencies ?? new Dictionary<string, string>();
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
