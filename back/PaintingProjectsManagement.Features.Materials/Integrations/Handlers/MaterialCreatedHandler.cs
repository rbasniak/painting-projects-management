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
        var e = envelope.Event;

        var integration = new MaterialCreatedV1(
            e.MaterialId,
            e.Name,
            e.PackageContent.Amount,
            e.PackageContent.Unit.ToString(),
            e.PackagePrice.Amount,
            e.PackagePrice.CurrencyCode
        );

        var integrationEnvelope = EventEnvelopeFactory.Wrap(
            integration,
            envelope.TenantId,
            envelope.Username,
            envelope.CorrelationId,
            envelope.EventId.ToString()
        );

        var id = await _outbox.Enqueue(integrationEnvelope, cancellationToken);
        await _scheduler.SeedDeliveriesAsync(id, integrationEnvelope.Name, integrationEnvelope.Version, cancellationToken);
    }
}

