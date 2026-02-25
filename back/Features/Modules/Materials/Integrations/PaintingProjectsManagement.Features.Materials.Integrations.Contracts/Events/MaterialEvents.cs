using System;

namespace PaintingProjectsManagement.Features.Materials.Abstractions;

// Fired once when a material is created
[EventName("materials.material-created", 1)]
public sealed record MaterialCreatedV1(
    Guid MaterialId,
    string Name,
    int CategoryId,
    string CategoryName,
    double PackageContentAmount,
    string PackageContentUnit,
    double PackagePriceAmount,
    string PackagePriceCurrency
 ) : IIntegrationEvent;

// Fired when a material is (soft-)deleted
[EventName("materials.material-deleted", 1)]
public sealed record MaterialDeletedV1(
    Guid MaterialId
 ) : IIntegrationEvent;

// Fired when a material changes
[EventName("materials.material-updated", 1)]
public sealed record MaterialUpdatedV1(
    Guid MaterialId,
    string Name,
    int CategoryId,
    string CategoryName,
    double PackageContentAmount,
    string PackageContentUnit,
    double PackagePriceAmount,
    string PackagePriceCurrency
 ) : IIntegrationEvent;
