using PaintingProjectsManagement.Features.Models.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Models;

internal sealed class ModelDeletedHandler : IDomainEventHandler<ModelDeleted>
{
    private readonly IIntegrationOutbox _outbox;

    public ModelDeletedHandler(IIntegrationOutbox outbox)
    {
        _outbox = outbox;
    }

    public async Task HandleAsync(EventEnvelope<ModelDeleted> envelope, CancellationToken cancellationToken)
    {
        var integrationEvent = new ModelDeletedV1(envelope.Event.ModelId);

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
