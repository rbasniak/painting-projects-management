using PaintingProjectsManagement.Features.Models.Abstractions;
using rbkApiModules.Commons.Core;
using System.Diagnostics;

namespace PaintingProjectsManagement.Features.Models;

internal sealed class ModelCreatedHandler : IDomainEventHandler<ModelCreated>
{
    private readonly IIntegrationOutbox _outbox;

    public ModelCreatedHandler(IIntegrationOutbox outbox)
    {
        _outbox = outbox;
    }

    public async Task HandleAsync(EventEnvelope<ModelCreated> envelope, CancellationToken cancellationToken)
    {
        using var span = EventsTracing.ActivitySource.StartActivity("integration.convert", ActivityKind.Internal);
        span?.SetTag("converter.source", nameof(ModelCreated));
        span?.SetTag("converter.target", nameof(ModelCreatedV1));

        var domainEvent = envelope.Event;

        var integrationEvent = new ModelCreatedV1(
            domainEvent.ModelId,
            domainEvent.Name,
            domainEvent.CategoryId,
            domainEvent.Artist,
            domainEvent.Franchise,
            domainEvent.Type.ToString(),
            domainEvent.Tags,
            domainEvent.Characters,
            domainEvent.BaseSize.ToString(),
            domainEvent.FigureSize.ToString(),
            domainEvent.NumberOfFigures,
            domainEvent.SizeInMb,
            domainEvent.MustHave,
            domainEvent.Score,
            domainEvent.PictureUrl
        );

        var integrationEnvelope = EventEnvelopeFactory.Wrap(
            integrationEvent,
            envelope.TenantId,
            envelope.Username,
            envelope.CorrelationId,
            envelope.EventId.ToString()
        );

        await _outbox.Enqueue(integrationEnvelope, cancellationToken);
    }
}
