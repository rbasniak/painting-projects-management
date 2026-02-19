namespace PaintingProjectsManagement.Features.Materials;

public sealed class Material : TenantEntity
{
    private Material() 
    { 
        // for EF
    } 

    public Material(string tenantId, string name, MaterialCategory category, Quantity packageContent, Money packagePrice)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(packageContent);
        ArgumentNullException.ThrowIfNull(packagePrice);

        Id = Guid.CreateVersion7();
        TenantId = tenantId;
        Name = name;
        PackageContent = packageContent;
        PackagePrice = packagePrice;

        RaiseDomainEvent(new MaterialCreated(Id, Name, category, PackageContent, PackagePrice));
    }

    public string Name { get; private set; } = string.Empty;

    public Quantity PackageContent { get; private set; } = default!;

    public Money PackagePrice { get; private set; } = default!;

    public double UnitPriceAmount => PackagePrice.Amount / PackageContent.Amount;

    public MaterialCategory Category { get; private set; } = MaterialCategory.Others;

    public PackageContentUnit UnitPriceUnit => PackageContent.Unit;

    public void UpdateDetails(string name, MaterialCategory category, Quantity packageContent, Money packagePrice)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(packageContent);
        ArgumentNullException.ThrowIfNull(packagePrice);

        var oldName = Name;
        var oldPackagePrice = PackagePrice;
        var oldPackageContent = PackageContent;
        var oldCategory = Category;

        Name = name;
        PackageContent = packageContent;
        PackagePrice = packagePrice;
        Category = category;

        if (oldPackageContent != PackageContent)
        {
            RaiseDomainEvent(new MaterialPackageContentChanged(Id, oldPackageContent, PackageContent));
        }

        if (oldPackagePrice != PackagePrice)
        {
            RaiseDomainEvent(new MaterialPackagePriceChanged(Id, oldPackagePrice, PackagePrice));
        }

        if (oldName != Name)
        {
            RaiseDomainEvent(new MaterialNameChanged(Id, name));
        }

        if (oldCategory != Category)
        {
            RaiseDomainEvent(new MaterialCategoryChanged(Id, oldCategory, Category));
        }
    }

    public void Delete()
    {
        RaiseDomainEvent(new MaterialDeleted(Id));
    }
}
