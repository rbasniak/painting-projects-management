using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials;

public sealed class MaterialDeletedHandler : IEventHandler<MaterialDeleted>
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
        var integration = new MaterialDeletedV1(envelope.Event.MaterialId);

        var integrationEnvelope = EventEnvelopeFactory.Wrap(
            integration,
            envelope.TenantId,
            envelope.Username,
            envelope.CorrelationId,
            envelope.EventId.ToString()
        );

        var eventId = _outbox.Enqueue(integrationEnvelope);
        await _scheduler.SeedDeliveriesAsync(eventId, integrationEnvelope.Name, integrationEnvelope.Version, cancellationToken);
    }
}

