using PaintingProjectsManagement.Features.Currency;

namespace PaintingProjectsManagement.Features.Projects;

public readonly record struct Money(double Amount, string Currency)
{
    private static string Norm(string c) =>
        string.IsNullOrWhiteSpace(c) ? throw new ArgumentNullException(nameof(c)) : c.ToUpperInvariant();

    public static Money operator +(Money a, Money b)
    {
        var ca = Norm(a.Currency);
        var cb = Norm(b.Currency);
        if (!string.Equals(ca, cb, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Currency mismatch: {a.Currency} vs {b.Currency}");
        }

        return new Money(a.Amount + b.Amount, ca);
    }

    public static Money operator -(Money a, Money b)
    {
        var ca = Norm(a.Currency);
        var cb = Norm(b.Currency);
        if (!string.Equals(ca, cb, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Currency mismatch: {a.Currency} vs {b.Currency}");
        }

        return new Money(a.Amount - b.Amount, ca);
    } 

    public static Money operator -(Money a) =>
        new Money(-a.Amount, Norm(a.Currency));
}

public static class MoneyExtensions
{
    public static async Task<Money> Convert(this Money money, string currency, ICurrencyConverter converter)
    {
        if (string.IsNullOrWhiteSpace(money.Currency))
        {
            throw new InvalidOperationException("Cannot convert money with an unspecified currency.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Target currency is required.", nameof(currency));
        }

        if (string.Equals(money.Currency, currency, StringComparison.OrdinalIgnoreCase))
        {
            return money;
        }

        var conversionRate = await converter.GetConversionRate(money.Currency, currency);

        return new Money(conversionRate * money.Amount, currency.Trim().ToUpperInvariant());
    }
}
