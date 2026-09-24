namespace PaintingProjectsManagement.Features.Materials;

// Fired once when a material is created
[EventName("Materials.MaterialCreated", 1)]
public sealed record MaterialCreated(
    Guid MaterialId,
    string Name,
    MaterialCategory Category,
    Quantity PackageContent,
    Money PackagePrice
) : IDomainEvent;

// Fired when a material is (soft-)deleted
[EventName("Materials.MaterialDeleted", 1)]
public sealed record MaterialDeleted(
    Guid MaterialId
) : IDomainEvent;

// Fired when package content (amount/unit) changes
[EventName("Materials.MaterialPackageContentChanged", 1)]
public sealed record MaterialPackageContentChanged(
    Guid MaterialId,
    Quantity OldContent,
    Quantity NewContent
) : IDomainEvent;

// Fired when package price changes
[EventName("Materials.MaterialPackagePriceChanged", 1)]
public sealed record MaterialPackagePriceChanged(
    Guid MaterialId,
    Money OldPrice,
    Money NewPrice
) : IDomainEvent;

// Fired when material is renamed
[EventName("Materials.MaterialNameChanged", 1)]
public sealed record MaterialNameChanged(
    Guid MaterialId,
    string newName
) : IDomainEvent;

// Fired when material category is changed
[EventName("Materials.MaterialCategoryChanged", 1)]
public sealed record MaterialCategoryChanged(
    Guid MaterialId,
    MaterialCategory OldCategory,
    MaterialCategory NewCategory
) : IDomainEvent;
