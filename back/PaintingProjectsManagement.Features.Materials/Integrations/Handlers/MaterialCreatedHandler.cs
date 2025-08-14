using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials;

internal sealed class MaterialCreatedHandler : IEventHandler<MaterialCreated>
{
    private readonly IIntegrationOutbox _outbox;

    public MaterialCreatedHandler(IIntegrationOutbox outbox)
    {
        _outbox = outbox;
    }

    public async Task Handle(EventEnvelope<MaterialCreated> envelope, CancellationToken cancellationToken)
    {
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

