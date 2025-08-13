namespace PaintingProjectsManagement.Features.Materials.Abstractions;

// Fired once when a material is created
[EventName($"Materials.Integration.MaterialCreated.v1", 1)]
public sealed record MaterialCreatedV1(
    Guid MaterialId,
    string Name,
    double PackageContentAmount,
    string PackageContentUnit,
    double PackagePriceAmount,
    string PackagePriceCurrency
) : IDomainEvent;

// Fired when a material is (soft-)deleted
[EventName("Materials.Integration.MaterialDeleted.v1", 1)]
public sealed record MaterialDeletedV1(
    Guid MaterialId
) : IDomainEvent;

// Fired when a material changes
[EventName("Materials.Integration.MaterialUpdated.v1", 1)]
public sealed record MaterialPackageContentChanged(
    Guid MaterialId,
    string Name,
    double PackageContentAmount,
    string PackageContentUnit,
    double PackagePriceAmount,
    string PackagePriceCurrency
) : IDomainEvent;
