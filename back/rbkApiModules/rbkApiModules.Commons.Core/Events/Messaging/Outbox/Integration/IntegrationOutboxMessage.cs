using System;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Represents an integration event stored in the integration outbox.
/// </summary>
public class IntegrationOutboxMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public short Version { get; set; }
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
    public string? TraceId { get; set; }
    public string? ParentSpanId { get; set; }
    public int? TraceFlags { get; set; }
    public string? TraceState { get; set; }
}
