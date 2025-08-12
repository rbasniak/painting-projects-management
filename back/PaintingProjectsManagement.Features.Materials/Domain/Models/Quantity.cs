namespace PaintingProjectsManagement.Features.Materials;

public enum PackageUnits
{
    Gram = 1,
    Milliliter = 2,
    Meter = 3,
    Each = 4
}

public sealed class Quantity
{
    private Quantity() { } // for EF

    public Quantity(double amount, PackageUnits unit)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        Amount = amount;
        Unit = unit;
    }

    public double Amount { get; private set; }

    public PackageUnits Unit { get; private set; }

    public override string ToString() => $"{Amount} {Unit}";
} 