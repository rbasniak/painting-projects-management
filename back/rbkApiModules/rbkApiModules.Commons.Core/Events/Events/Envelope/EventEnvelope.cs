using System;

namespace rbkApiModules.Commons.Core;

// TODO: review classes to add required and remove default!
public sealed class EventEnvelope<TEvent>
{
    public Guid EventId { get; init; }
    public string Name { get; init; } = default!;
    public short Version { get; init; }
    public DateTime OccurredUtc { get; init; }
    public string TenantId { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string? CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public TEvent Event { get; init; } = default!;
}  