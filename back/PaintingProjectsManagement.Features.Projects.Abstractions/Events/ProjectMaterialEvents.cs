namespace PaintingProjectsManagement.Features.Projects.Abstractions;

[EventName("Projects.ProjectMaterialAdded", 1)]
public sealed record ProjectMaterialAdded(Guid ProjectId, Guid MaterialId, double Quantity);

[EventName("Projects.ProjectMaterialQuantityChanged", 1)]
public sealed record ProjectMaterialQuantityChanged(Guid ProjectId, Guid MaterialId, double Quantity);

[EventName("Projects.ProjectMaterialRemoved", 1)]
public sealed record ProjectMaterialRemoved(Guid ProjectId, Guid MaterialId); 