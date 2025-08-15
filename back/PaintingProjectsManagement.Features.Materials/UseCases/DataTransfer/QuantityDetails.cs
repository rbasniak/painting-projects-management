namespace PaintingProjectsManagement.Features.Materials;

public class QuantityDetails
{
    public EnumReference Unit { get; init; } = EnumReference.Empty;
    public double Amount { get; init; } = 0;

    public static QuantityDetails Empty => new()
    {
        Unit = EnumReference.Empty,
        Amount = 0
    };
}
