using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials;

public sealed class MaterialDeletedHandler : IEventHandler<MaterialDeleted>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<OutboxOptions> _outboxOptions;

    public MaterialDeletedHandler(IServiceScopeFactory scopeFactory, IOptions<OutboxOptions> outboxOptions)
    {
        _scopeFactory = scopeFactory;
        _outboxOptions = outboxOptions;
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

        var payload = JsonEventSerializer.Serialize(integrationEnvelope);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = _outboxOptions.Value.ResolveDbContext!(scope.ServiceProvider);

        dbContext.Set<OutboxMessage>().Add(new OutboxMessage
        {
            Id = integrationEnvelope.EventId,
            Name = integrationEnvelope.Name,
            Version = integrationEnvelope.Version,
            TenantId = integrationEnvelope.TenantId,
            Username = integrationEnvelope.Username,
            OccurredUtc = integrationEnvelope.OccurredUtc,
            CorrelationId = integrationEnvelope.CorrelationId,
            CausationId = integrationEnvelope.CausationId,
            Payload = payload,
            CreatedUtc = DateTime.UtcNow,
            ProcessedUtc = null,
            Attempts = 0
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

