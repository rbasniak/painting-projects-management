using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace rbkApiModules.Commons.Core;

public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IRequestContext _requestContext;
    public OutboxSaveChangesInterceptor(IRequestContext requestContext)
    {
        _requestContext = requestContext;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not DbContext context)
        {
            return base.SavingChanges(eventData, result);
        }

        PersistDomainEventsToOutbox(context);
        
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not DbContext context)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        PersistDomainEventsToOutbox(context);
        
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void PersistDomainEventsToOutbox(DbContext context)
    {
        var aggregatesWithEvents = context.ChangeTracker.Entries()
           .Where(e => e.Entity is AggregateRoot aggregateRoot && aggregateRoot.GetDomainEvents().Count > 0)
           .Select(e => (AggregateRoot)e.Entity)
           .ToArray();

        var now = DateTime.UtcNow;

        foreach (var aggregate in aggregatesWithEvents)
        {
            var domainEvents = aggregate.GetDomainEvents();

            if (domainEvents.Count == 0)
            {
                continue;
            }

            foreach (var domainEvent in domainEvents)
            {
                var envelope = EventEnvelopeFactory.Wrap(domainEvent, _requestContext.TenantId, _requestContext.Username, _requestContext.CorrelationId, _requestContext.CausationId);
                var payload = JsonEventSerializer.Serialize(envelope);

                context.Set<OutboxDomainMessage>().Add(new OutboxDomainMessage
                {
                    Id = envelope.EventId,
                    Name = envelope.Name,
                    Version = envelope.Version,
                    TenantId = envelope.TenantId,
                    Username = envelope.Username,
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