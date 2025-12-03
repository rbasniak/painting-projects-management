using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Materials.UseCases.Integrations;

internal sealed class MaterialDeletedHandler : IDomainEventHandler<MaterialDeleted>
{
    private readonly IIntegrationOutbox _outbox;

    public MaterialDeletedHandler(IIntegrationOutbox outbox)
    {
        _outbox = outbox;
    }

    public async Task HandleAsync(EventEnvelope<MaterialDeleted> envelope, CancellationToken cancellationToken)
    {
        var integrationEvent = new MaterialDeletedV1(envelope.Event.MaterialId);

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

