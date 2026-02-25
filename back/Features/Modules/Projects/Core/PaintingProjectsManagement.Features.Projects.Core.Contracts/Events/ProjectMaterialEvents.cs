namespace PaintingProjectsManagement.Features.Projects.Abstractions;

[EventName("Projects.ProjectMaterialAdded", 1)]
public sealed record ProjectMaterialAdded(Guid ProjectId, Guid MaterialId, double Quantity) : IDomainEvent;


[EventName("Projects.ProjectMaterialQuantityChanged", 1)]
public sealed record ProjectMaterialQuantityChanged(Guid ProjectId, Guid MaterialId, double Quantity) : IDomainEvent;


[EventName("Projects.ProjectMaterialRemoved", 1)]
public sealed record ProjectMaterialRemoved(Guid ProjectId, Guid MaterialId) : IDomainEvent;
