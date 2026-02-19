namespace PaintingProjectsManagement.Features.Materials.UseCases.Web;

public class QuantityDetails
{
    public EnumReference Unit { get; init; } = EnumReference.Empty;
    public double Amount { get; init; } = 0;

    public override string ToString() => $"{Amount} {Unit.Value}";

    public static QuantityDetails Empty => new()
    {
        Unit = EnumReference.Empty,
        Amount = 0
    };
}
