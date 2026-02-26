using System.Net.Http.Json;

namespace PaintingProjectsManagement.UI.Modules.Materials;

public interface IMaterialsService
{
    Task<IReadOnlyCollection<MaterialDetails>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CurrencyOption>> GetCurrenciesAsync(CancellationToken cancellationToken);

    Task<MaterialDetails> CreateAsync(
      CreateMaterialRequest request,
      CancellationToken cancellationToken);

    Task<MaterialDetails> UpdateAsync(
      UpdateMaterialRequest request,
      CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public class MaterialsService : IMaterialsService
{
    private static readonly IReadOnlyCollection<CurrencyOption> DefaultCurrencies =
    [
        new CurrencyOption { Code = "USD", Name = "US Dollar" }
    ];

    private readonly HttpClient _httpClient;

    public MaterialsService(HttpClient httpClient) => this._httpClient = httpClient;

    public async Task<IReadOnlyCollection<MaterialDetails>> GetAllAsync(
      CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("api/materials", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MaterialDetails>>();
            return result ?? Array.Empty<MaterialDetails>();
        }

        return Array.Empty<MaterialDetails>();
    }

    public async Task<IReadOnlyCollection<CurrencyOption>> GetCurrenciesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/currencies", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return DefaultCurrencies;
            }

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<CurrencyOption>>(cancellationToken: cancellationToken);

            var currencies = result?
                .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                .Select(x => x with { Code = x.Code.Trim().ToUpperInvariant() })
                .OrderBy(x => x.Code, StringComparer.Ordinal)
                .ToArray();

            return currencies is { Length: > 0 } ? currencies : DefaultCurrencies;
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return DefaultCurrencies;
        }
    }

    public async Task<MaterialDetails> CreateAsync(
      CreateMaterialRequest request,
      CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/materials", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<MaterialDetails>();
            return result ?? new MaterialDetails();
        }

        return new MaterialDetails();
    }

    public async Task<MaterialDetails> UpdateAsync(
      UpdateMaterialRequest request,
      CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync("api/materials", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<MaterialDetails>();
            return result ?? new MaterialDetails();
        }

        return new MaterialDetails();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _httpClient.DeleteAsync($"api/materials/{id}", cancellationToken);
    }
}
