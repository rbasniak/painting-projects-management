namespace PaintingProjectsManagement.Features.Projects.Abstractions;

[EventName("Projects.BuildingStepAddedToTheProject", 1)]
public sealed record BuildingStepAddedToTheProject(
    Guid ProjectId,
    int StepId,
    DateTime StartDate,
    DateTime EndDate
) : IDomainEvent;
