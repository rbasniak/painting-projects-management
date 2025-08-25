using PaintingProjectsManagement.UI.Modules.Shared;
using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.UI.Modules.Materials;

public record MaterialDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
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
    public EnumReference Unit { get; set; } = new(0, string.Empty);
}

public enum PackageContentUnit
{
    Gram = 10,
    Milliliter = 20,
    Meter = 30,
    Each = 40
}