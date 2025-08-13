using System;
using System.Threading;
using System.Threading.Tasks;

namespace rbkApiModules.Commons.Core;

public interface IIntegrationDeliveryScheduler
{
    Task SeedDeliveriesAsync(Guid eventId, string eventName, int version, CancellationToken ct = default);
}
