using System.Text.Json.Serialization;
using PaintingProjectsManagement.Features.Materials;
using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Materials;

public record MaterialDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; } = (int)MaterialCategory.Others;

    [JsonIgnore]
    public MaterialCategory Category
    {
        get => (MaterialCategory)CategoryId;
        set => CategoryId = (int)value;
    }

    public MoneyDetails PackagePrice { get; set; } = new();
    public QuantityDetails PackageContent { get; set; } = new();
}

public record MoneyDetails
{
    public double Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
}

public record QuantityDetails
{
    public double Amount { get; set; }
    public EnumReference Unit { get; set; } = EnumReference.FromValue(PackageContentUnit.Gram);
}

public enum PackageContentUnit
{
    Gram = 10,
    Mililiter = 20,
    Meter = 30,
    Each = 40
}
