namespace PaintingProjectsManagement.Features.Materials;

// Fired once when a material is created
[EventName("Materials.MaterialCreated", 1)]
internal sealed record MaterialCreated(
    Guid MaterialId,
    string Name,
    Quantity PackageContent,
    Money PackagePrice
) : IDomainEvent;

// Fired when a material is (soft-)deleted
[EventName("Materials.MaterialDeleted", 1)]
internal sealed record MaterialDeleted(
    Guid MaterialId
) : IDomainEvent;

// Fired when package content (amount/unit) changes
[EventName("Materials.MaterialPackageContentChanged", 1)]
internal sealed record MaterialPackageContentChanged(
    Guid MaterialId,
    Quantity OldContent,
    Quantity NewContent
) : IDomainEvent;

// Fired when package price changes
[EventName("Materials.MaterialPackagePriceChanged", 1)]
internal sealed record MaterialPackagePriceChanged(
    Guid MaterialId,
    Money OldPrice,
    Money NewPrice
) : IDomainEvent;