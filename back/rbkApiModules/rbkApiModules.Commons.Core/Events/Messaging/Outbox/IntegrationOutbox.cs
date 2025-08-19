using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Default implementation of <see cref="IIntegrationOutbox"/> which stores
/// integration events in the database.
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

    public async Task<Guid> Enqueue<T>(EventEnvelope<T> envelope, CancellationToken ct = default)
    {
        using var activity = EventsActivities.Source.StartActivity("enqueue_outbox", ActivityKind.Internal);
        activity?.SetTag("event.name", envelope.Name);
        activity?.SetTag("event.version", envelope.Version);
        activity?.SetTag("tenant", envelope.TenantId ?? string.Empty);

        using var scope = _scopeFactory.CreateScope();
        var db = _options.ResolveDbContext!(scope.ServiceProvider);

        var message = new OutboxIntegrationEvent
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
            Attempts = 0
        };

        db.Set<OutboxIntegrationEvent>().Add(message);
        await db.SaveChangesAsync(ct);
        return message.Id;
    }
}
