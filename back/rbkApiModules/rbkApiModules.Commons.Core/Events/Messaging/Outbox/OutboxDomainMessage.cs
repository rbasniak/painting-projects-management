using System;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Represents an event that originated from the domain and should be
/// dispatched by the outbox dispatcher. These messages are persisted in the
/// <c>OutboxDomainMessages</c> table and are processed independently from
/// integration events.
/// </summary>
public class OutboxDomainMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int Version { get; set; }
    public string TenantId { get; set; } = default!;
    public DateTime OccurredUtc { get; set; }
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public string Payload { get; set; } = default!;
    public DateTime CreatedUtc { get; set; }
    public DateTime? ProcessedUtc { get; set; }
    public int Attempts { get; set; }
    public string Username { get; set; } = default!;
    public DateTime? DoNotProcessBeforeUtc { get; set; }
} 