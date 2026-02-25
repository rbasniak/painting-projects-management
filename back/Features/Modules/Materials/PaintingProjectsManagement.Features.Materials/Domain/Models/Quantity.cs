using FluentValidation.Results;

namespace PaintingProjectsManagement.Features.Materials;

public sealed record Quantity
{
    private Quantity() 
    {
        // for EF
    }

    public Quantity(double amount, PackageContentUnit unit)
    {
        if (amount <= 0)
        {
            throw new ValidationException([new ValidationFailure(nameof(unit), "Amount must be positive.")]);
        }

        Amount = amount;
        Unit = unit;
    }

    public double Amount { get; private set; }

    public PackageContentUnit Unit { get; private set; }

    public override string ToString() => $"{Amount} {Unit}";
} 