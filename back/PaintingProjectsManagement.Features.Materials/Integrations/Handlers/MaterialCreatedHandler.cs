using PaintingProjectsManagement.Features.Materials.Abstractions;
using System.Diagnostics;

namespace PaintingProjectsManagement.Features.Materials;

internal sealed class MaterialCreatedHandler : IDomainEventHandler<MaterialCreated>
{
    private readonly IIntegrationOutbox _outbox;

    public MaterialCreatedHandler(IIntegrationOutbox outbox)
    {
        _outbox = outbox;
    }

    public async Task HandleAsync(EventEnvelope<MaterialCreated> envelope, CancellationToken cancellationToken)
    {
        using var span = EventsTracing.ActivitySource.StartActivity("integration.convert", ActivityKind.Internal);
        span?.SetTag("converter.source", nameof(MaterialCreated));
        span?.SetTag("converter.target", nameof(MaterialCreatedV1));

        var domainEvent = envelope.Event;

        var integrationEvent = new MaterialCreatedV1(
            domainEvent.MaterialId,
            domainEvent.Name,
            domainEvent.PackageContent.Amount,
            domainEvent.PackageContent.Unit.ToString(),
            domainEvent.PackagePrice.Amount,
            domainEvent.PackagePrice.CurrencyCode
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

