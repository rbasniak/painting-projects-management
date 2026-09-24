namespace PaintingProjectsManagement.Features.Projects.Abstractions;

[EventName("Projects.BuildingStepAdded", 1)]
public sealed record BuildingStepAddedToTheProject(
    Guid ProjectId,
    int StepId,
    DateTime StartDate,
    DateTime EndDate
) : IDomainEvent;

[EventName("Projects.BuildingStepUpdated", 1)]
public sealed record BuildingStepUpdated(
    Guid ProjectId,
    Guid StepId,
    DateTime StartDate,
    DateTime EndDate
) : IDomainEvent;

[EventName("Projects.BuildingStepRemoved", 1)]
public sealed record BuildingStepRemoved(
    Guid ProjectId,
    Guid StepId
) : IDomainEvent;
