using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials;

internal sealed class MaterialDeletedHandler : IEventHandler<MaterialDeleted>
{
    private readonly IIntegrationOutbox _outbox;
    private readonly IIntegrationDeliveryScheduler _scheduler;

    public MaterialDeletedHandler(IIntegrationOutbox outbox, IIntegrationDeliveryScheduler scheduler)
    {
        _outbox = outbox;
        _scheduler = scheduler;
    }

    public async Task Handle(EventEnvelope<MaterialDeleted> envelope, CancellationToken cancellationToken)
    {
        var integrationEvent = new MaterialDeletedV1(envelope.Event.MaterialId);

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

