using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PaintingProjectsManagement.Features.Currency;
using PaintingProjectsManagement.Testing.Core;

namespace PaintingProjectsManagement.Features.Projects.Tests;

// IMPORTANT: if using Podman instead of Docker, make sure Docker Compatibility is enabled in Podman settings.

public class TestingServer : BaseApplicationTestingServer<Program>
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.RemoveAll<ICurrencyConverter>();
        services.AddSingleton<ICurrencyConverter, DeterministicTestCurrencyConverter>();
    }

    /// <summary>
    /// Avoids outbound HTTP to Frankfurter during cost calculation; tests assert behavior, not FX rates.
    /// </summary>
    private sealed class DeterministicTestCurrencyConverter : ICurrencyConverter
    {
        public Task<double> GetConversionRate(string fromCurrency, string toCurrency)
        {
            return Task.FromResult(1.0);
        }

        public Task<Dictionary<string, string>> GetAvailableCurrencies()
        {
            return Task.FromResult(new Dictionary<string, string>());
        }
    }
}
