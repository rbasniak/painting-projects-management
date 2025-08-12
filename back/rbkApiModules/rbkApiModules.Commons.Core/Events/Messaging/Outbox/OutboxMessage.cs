using System;

namespace rbkApiModules.Commons.Core;

public class OutboxMessage
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
} 