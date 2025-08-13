namespace PaintingProjectsManagement.Features.Materials;

public sealed class Material : TenantEntity
{
    private Material() 
    { 
        // for EF
    } 

    public Material(string tenantId, string name, Quantity packageContent, Money packagePrice)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(packageContent);
        ArgumentNullException.ThrowIfNull(packagePrice);

        TenantId = tenantId;
        Name = name;
        PackageContent = packageContent;
        PackagePrice = packagePrice;
    }

    public string Name { get; private set; } = string.Empty;

    public Quantity PackageContent { get; private set; } = default!;

    public Money PackagePrice { get; private set; } = default!;

    public double UnitPriceAmount => PackagePrice.Amount / PackageContent.Amount;

    public PackageUnits UnitPriceUnit => PackageContent.Unit;

    public void UpdateDetails(string name, Quantity packageContent, Money packagePrice)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(packageContent);
        ArgumentNullException.ThrowIfNull(packagePrice);

        Name = name;
        PackageContent = packageContent;
        PackagePrice = packagePrice;
    }
}
