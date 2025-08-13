using FluentValidation.Results;

namespace PaintingProjectsManagement.Features.Materials;

public sealed class Quantity
{
    private Quantity() 
    {
        // for EF
    }

    public Quantity(double amount, PackageUnit unit)
    {
        if (amount <= 0)
        {
            throw new ValidationException([new ValidationFailure(nameof(unit), "Amount must be positive.")]);
        }

        Amount = amount;
        Unit = unit;
    }

    public double Amount { get; private set; }

    public PackageUnit Unit { get; private set; }

    public override string ToString() => $"{Amount} {Unit}";
} 