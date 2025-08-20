using System;

namespace rbkApiModules.Commons.Core;

public sealed record EventEnvelope<TEvent>
{
    public required Guid EventId { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required short Version { get; init; } = 1;
    public required DateTime OccurredUtc { get; init; }
    public required string TenantId { get; init; } = string.Empty;
    public required string Username { get; init; } = string.Empty;
    public required string? CorrelationId { get; init; } = string.Empty;
    public required string? CausationId { get; init; } = string.Empty;
    public required TEvent Event { get; init; }
}