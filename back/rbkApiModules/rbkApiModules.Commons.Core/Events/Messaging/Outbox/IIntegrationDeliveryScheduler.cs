using System;
using System.Threading;
using System.Threading.Tasks;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Responsible for creating delivery records for the current subscribers of a
/// given integration event.
/// </summary>
public interface IIntegrationDeliveryScheduler
{
    Task SeedDeliveriesAsync(Guid eventId, string eventName, int version, CancellationToken ct);
}
