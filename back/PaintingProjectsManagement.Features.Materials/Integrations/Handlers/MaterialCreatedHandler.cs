using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials;

internal sealed class MaterialCreatedHandler : IEventHandler<MaterialCreated>
{
    private readonly IIntegrationOutbox _outbox;
    private readonly IIntegrationDeliveryScheduler _scheduler;

    public MaterialCreatedHandler(IIntegrationOutbox outbox, IIntegrationDeliveryScheduler scheduler)
    {
        _outbox = outbox;
        _scheduler = scheduler;
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

        var id = await _outbox.Enqueue(integrationEnvelope, cancellationToken);

        await _scheduler.SeedDeliveriesAsync(id, integrationEnvelope.Name, integrationEnvelope.Version, cancellationToken);
    }
}

