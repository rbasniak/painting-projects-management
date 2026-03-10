using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PaintingProjectsManagement.Features.Currency;

internal static class CurrencyEndpoint
{
    public static IEndpointRouteBuilder MapCurrencyEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/currencies", async (ICurrencyCatalog currencyCatalog, CancellationToken cancellationToken) =>
        {
            try
            {
                var currencies = await currencyCatalog.ListAvailableCurrenciesAsync(cancellationToken);

                return Results.Ok(currencies);
            }
            catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException)
            {
                return Results.Problem(
                    title: "Currency list unavailable",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        })
        .Produces<IReadOnlyCollection<CurrencyOption>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
        .RequireAuthorization()
        .WithName("List Currencies")
        .WithTags("Currencies");

        return app;
    }
}
