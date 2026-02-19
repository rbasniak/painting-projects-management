namespace PaintingProjectsManagement.Features.Models;

// DOCS: events can be thick or thin, thin events are ok for domain events, thick events are better for integration events. But we can also use thick events for domain events if needed.

[EventName("Models.ModelCreated", 1)]
public sealed record ModelCreated(
    Guid ModelId,
    string ModelName
) : IDomainEvent;

[EventName("Models.ModelDeleted", 1)]
public sealed record ModelDeleted(
    Guid ModelId
) : IDomainEvent;

[EventName("Models.ModelDetailsChanged", 1)]
public sealed record ModelDetailsChanged(
    Guid ModelId
) : IDomainEvent;

[EventName("Models.ModelCoverPictureChanged", 1)]
public sealed record ModelCoverPictureChanged(
    Guid ModelId
) : IDomainEvent;

[EventName("Models.ModelPicturesChanged", 1)]
public sealed record ModelPicturesChanged(
    Guid ModelId
) : IDomainEvent;

[EventName("Models.ModelRated", 1)]
public sealed record ModelRated(
    Guid ModelId,
    int Score
) : IDomainEvent;

[EventName("Models.ModelMustHaveChanged", 1)]
public sealed record ModelMustHaveChanged(
    Guid ModelId,
    bool MustHave
) : IDomainEvent;

[EventName("Models.ModelBaseSizeChanged", 1)]
public sealed record ModelBaseSizeChanged(
    Guid ModelId,
    BaseSize BaseSize
) : IDomainEvent;

[EventName("Models.ModelFigureSizeChanged", 1)]
public sealed record ModelFigureSizeChanged(
    Guid ModelId,
    FigureSize FigureSize
) : IDomainEvent;

[EventName("Models.ModelNumberOfFiguresChanged", 1)]
public sealed record ModelNumberOfFiguresChanged(
    Guid ModelId,
    int NumberOfFigures
) : IDomainEvent;
