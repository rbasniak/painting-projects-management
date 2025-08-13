using System;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Represents the delivery status of an integration event for a specific
/// subscriber.
/// </summary>
public class IntegrationDelivery
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Subscriber { get; set; } = default!;
    public DateTime? ProcessedUtc { get; set; }
    public int Attempts { get; set; }
}
