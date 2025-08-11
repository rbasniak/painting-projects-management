using System;
using System.Reflection;

namespace rbkApiModules.Commons.Core;

public static class EventEnvelopeFactory
{
    public static EventEnvelope<TEvent> Wrap<TEvent>(TEvent @event, string tenantId, string? correlationId = null, string? causationId = null)
    {
        if (@event is null) throw new ArgumentNullException(nameof(@event));
        if (tenantId is null) throw new ArgumentNullException(nameof(tenantId));

        var attribute = typeof(TEvent).GetCustomAttribute<EventNameAttribute>()
            ?? throw new InvalidOperationException($"Missing [EventName] on {typeof(TEvent).FullName}");

        return new EventEnvelope<TEvent>
        {
            EventId = Guid.NewGuid(),
            Name = attribute.Name,
            Version = attribute.Version,
            OccurredUtc = DateTime.UtcNow,
            TenantId = tenantId,
            CorrelationId = correlationId,
            CausationId = causationId,
            Event = @event
        };
    }
} 