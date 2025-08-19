using System;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Represents an outbox message that originated from a domain event. These
/// messages are stored in the <c>OutboxDomainMessages</c> table and later
/// dispatched by the outbox dispatcher.
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
    public string? TraceId { get; set; }
    public string? ParentSpanId { get; set; }
    public int? TraceFlags { get; set; }
    public string? TraceState { get; set; }
    public string Payload { get; set; } = default!;
    public DateTime CreatedUtc { get; set; }
    public DateTime? ProcessedUtc { get; set; }
    public int Attempts { get; set; }
    public string Username { get; set; } = default!;
    public DateTime? DoNotProcessBeforeUtc { get; set; }
} 