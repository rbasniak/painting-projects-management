namespace PaintingProjectsManagement.Features.Models.Abstractions;

// Fired once when a model is created
[EventName("models.model-created", 1)]
public sealed record ModelCreatedV1(
    Guid ModelId,
    string Name,
    Guid CategoryId,
    string Artist,
    string Franchise,
    string ModelType,
    string[] Tags,
    string[] Characters,
    string BaseSize,
    string FigureSize,
    int NumberOfFigures,
    int SizeInMb,
    bool MustHave,
    int Score,
    string? PictureUrl
) : IIntegrationEvent;

// Fired when a model is (soft-)deleted
[EventName("models.model-deleted", 1)]
public sealed record ModelDeletedV1(
    Guid ModelId
) : IIntegrationEvent;

// Fired when a model changes
[EventName("models.model-updated", 1)]
public sealed record ModelUpdatedV1(
    Guid ModelId,
    string Name,
    Guid CategoryId,
    string Artist,
    string Franchise,
    string ModelType,
    string[] Tags,
    string[] Characters,
    string BaseSize,
    string FigureSize,
    int NumberOfFigures,
    int SizeInMb,
    bool MustHave,
    int Score,
    string? PictureUrl
) : IIntegrationEvent;
