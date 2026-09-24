namespace PaintingProjectsManagement.Features.Projects;

public sealed record Material
{
    public required string Tenant { get; init; } = string.Empty!;
    public required Guid Id { get; init; }
    public required string Name { get; set; } = string.Empty;
    public required int CategoryId { get; set; }  
    public required string CategoryName { get; set; } = string.Empty!;
    public required Money PricePerUnit { get; set; }
    public required MaterialUnit Unit { get; set; } = default!;
    public required DateTime UpdatedUtc { get; set; }
}

public enum MaterialUnit
{
    Drop = 1,
    Unit = 2,
    Centimeter = 3,
    Meter = 4,
    Gram = 5,
    Kilogram = 6,
    Liter = 7,
    Mililiter = 8,
    Spray = 9
}

public readonly record struct Quantity(double Value, MaterialUnit Unit)
{
    public static Quantity operator *(Quantity quantity, double multiplier)
    {
        return new Quantity(quantity.Value * multiplier, quantity.Unit);
    }

    public static Quantity operator *(double multiplier, Quantity quantity)
    {
        return quantity * multiplier;
    }
}
