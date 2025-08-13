using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Default implementation of <see cref="IIntegrationOutbox"/> using EF Core.
/// </summary>
public class IntegrationOutbox : IIntegrationOutbox
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxOptions _options;

    public IntegrationOutbox(IServiceScopeFactory scopeFactory, IOptions<OutboxOptions> options)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    public Guid Enqueue<T>(EventEnvelope<T> envelope)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = _options.ResolveDbContext!(scope.ServiceProvider);

        var entity = new OutboxIntegrationEvent
        {
            Id = envelope.EventId,
            Name = envelope.Name,
            Version = envelope.Version,
            TenantId = envelope.TenantId,
            Username = envelope.Username,
            OccurredUtc = envelope.OccurredUtc,
            CorrelationId = envelope.CorrelationId,
            CausationId = envelope.CausationId,
            Payload = JsonEventSerializer.Serialize(envelope),
            CreatedUtc = DateTime.UtcNow,
            ProcessedUtc = null,
            Attempts = 0
        };

        db.Set<OutboxIntegrationEvent>().Add(entity);
        db.SaveChanges();

        return entity.Id;
    }
}
