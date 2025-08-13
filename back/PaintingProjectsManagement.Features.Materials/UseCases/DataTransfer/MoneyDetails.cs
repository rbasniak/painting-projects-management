namespace PaintingProjectsManagement.Features.Materials;

public class MoneyDetails
{
    public string CurrencyCode { get; init; } = string.Empty;
    public double Amount { get; init; } = 0;

    public static MoneyDetails Empty => new()
    { 
        CurrencyCode = string.Empty, 
        Amount = 0 
    };
}
