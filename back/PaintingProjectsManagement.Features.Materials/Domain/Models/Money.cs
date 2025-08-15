using FluentValidation.Results;

namespace PaintingProjectsManagement.Features.Materials;

public sealed class Money
{
    private Money()
    {
        // for EF
    }

    public Money(double amount, string currencyCode)
    {
        ArgumentNullException.ThrowIfNull(currencyCode);

        if (amount <= 0)
        {
            throw new ValidationException([new ValidationFailure(nameof(amount), "Amount must be positive.")]);
        }

        if (string.IsNullOrEmpty(currencyCode) || currencyCode.Length != 3)
        {
            throw new ValidationException([new ValidationFailure(nameof(currencyCode), "ISO 4217 code, length 3.")]);
        }

        Amount = amount;
        CurrencyCode = currencyCode.ToUpperInvariant();
    }

    public double Amount { get; private set; }

    public string CurrencyCode { get; private set; } = string.Empty;

    public override string ToString() => $"{Amount} {CurrencyCode}";
} 