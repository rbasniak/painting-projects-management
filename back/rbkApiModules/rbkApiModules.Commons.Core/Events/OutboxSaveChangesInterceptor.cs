using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace rbkApiModules.Commons.Core;

public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public OutboxSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not DbContext context) return base.SavingChanges(eventData, result);
        PersistDomainEventsToOutbox(context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not DbContext context) return base.SavingChangesAsync(eventData, result, cancellationToken);
        PersistDomainEventsToOutbox(context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void PersistDomainEventsToOutbox(DbContext context)
    {
        var aggregatesWithEvents = context.ChangeTracker.Entries()
            .Where(e => e.Entity is AggregateRoot)
            .Select(e => (AggregateRoot)e.Entity)
            .ToList();

        if (aggregatesWithEvents.Count == 0) return;

        // TODO: what will happen if running outside of HTTP context?
        var tenantId = _httpContextAccessor.GetTenant();
        var now = DateTime.UtcNow;

        foreach (var aggregate in aggregatesWithEvents)
        {
            var domainEvents = aggregate.GetDomainEvents();
            if (domainEvents.Count == 0) continue;

            foreach (var domainEvent in domainEvents)
            {
                var envelope = EventEnvelopeFactory.Wrap(domainEvent, tenantId);
                var payload = JsonEventSerializer.Serialize(envelope);

                context.Set<OutboxMessage>().Add(new OutboxMessage
                {
                    Id = envelope.EventId,
                    Name = envelope.Name,
                    Version = envelope.Version,
                    TenantId = envelope.TenantId,
                    OccurredUtc = envelope.OccurredUtc,
                    CorrelationId = envelope.CorrelationId,
                    CausationId = envelope.CausationId,
                    Payload = payload,
                    CreatedUtc = now,
                    ProcessedUtc = null,
                    Attempts = 0
                });
            }

            aggregate.ClearDomainEvents();
        }
    }
} 