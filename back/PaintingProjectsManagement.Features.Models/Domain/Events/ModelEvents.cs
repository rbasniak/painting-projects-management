namespace PaintingProjectsManagement.Features.Models;

// Fired once when a model is created
[EventName("Models.ModelCreated", 1)]
public sealed record ModelCreated(
    Guid ModelId,
    string Name,
    Guid CategoryId,
    string Artist,
    string Franchise,
    ModelType Type,
    string[] Tags,
    string[] Characters,
    BaseSize BaseSize,
    FigureSize FigureSize,
    int NumberOfFigures,
    int SizeInMb,
    bool MustHave,
    int Score,
    string? PictureUrl
) : IDomainEvent;

// Fired when a model is (soft-)deleted
[EventName("Models.ModelDeleted", 1)]
public sealed record ModelDeleted(
    Guid ModelId
) : IDomainEvent;

// Fired when model details change
[EventName("Models.ModelDetailsChanged", 1)]
public sealed record ModelDetailsChanged(
    Guid ModelId
) : IDomainEvent;

// Fired when model picture changes
[EventName("Models.ModelPictureChanged", 1)]
public sealed record ModelPictureChanged(
    Guid ModelId
) : IDomainEvent;

// Fired when model rating changes
[EventName("Models.ModelRated", 1)]
public sealed record ModelRated(
    Guid ModelId,
    int Score
) : IDomainEvent;

// Fired when model must-have status changes
[EventName("Models.ModelMustHaveChanged", 1)]
public sealed record ModelMustHaveChanged(
    Guid ModelId,
    bool MustHave
) : IDomainEvent;
