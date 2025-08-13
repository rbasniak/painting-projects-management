using System;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Represents the delivery of an integration event to a specific subscriber.
/// </summary>
public class IntegrationDelivery
{
    public Guid EventId { get; set; }
    public string Subscriber { get; set; } = default!;
    public DateTime? ProcessedUtc { get; set; }
    public int Attempts { get; set; }
    public DateTime? DoNotProcessBeforeUtc { get; set; }

    public OutboxIntegrationEvent Event { get; set; } = default!;
}
