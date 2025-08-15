using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Materials;
public class MaterialDetails
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public MoneyDetails PackagePrice { get; init; } = MoneyDetails.Empty;
    public QuantityDetails PackagetContent { get; init; } = QuantityDetails.Empty;

    public static MaterialDetails FromModel(Material material)
    {
        return new MaterialDetails
        {
            Id = material.Id,
            Name = material.Name,
            PackagePrice = new MoneyDetails
            {
                Amount = material.PackagePrice.Amount,
                CurrencyCode = material.PackagePrice.CurrencyCode
            },
            PackagetContent = new QuantityDetails
            {
                Amount = material.PackageContent.Amount,
                Unit = new EnumReference(material.PackageContent.Unit)
            }
        };
    }
}
