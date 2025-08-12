namespace PaintingProjectsManagement.Features.Materials;

public sealed class Money
{
    private Money() { } // for EF

    public Money(double amount, string currencyCode)
    {
        ArgumentNullException.ThrowIfNull(currencyCode);
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        if (currencyCode.Length != 3) throw new ArgumentException("ISO 4217 code, length 3.", nameof(currencyCode));
        Amount = amount;
        CurrencyCode = currencyCode.ToUpperInvariant();
    }

    public double Amount { get; private set; }

    public string CurrencyCode { get; private set; } = string.Empty;

    public override string ToString() => $"{Amount} {CurrencyCode}";
} 