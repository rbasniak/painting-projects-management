using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Projects;

public record ProjectDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; 
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

public enum MaterialUnit
{
    Gram = 10,
    Milliliter = 20,
    Meter = 30,
    Each = 40
} 