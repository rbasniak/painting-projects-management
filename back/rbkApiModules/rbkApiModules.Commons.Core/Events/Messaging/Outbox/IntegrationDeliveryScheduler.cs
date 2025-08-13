using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Default implementation that creates delivery records for all subscribers of
/// a given integration event.
/// </summary>
public class IntegrationDeliveryScheduler : IIntegrationDeliveryScheduler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IIntegrationSubscriberRegistry _registry;
    private readonly OutboxOptions _options;

    public IntegrationDeliveryScheduler(IServiceScopeFactory scopeFactory,
        IIntegrationSubscriberRegistry registry,
        IOptions<OutboxOptions> options)
    {
        _scopeFactory = scopeFactory;
        _registry = registry;
        _options = options.Value;
    }

    public async Task SeedDeliveriesAsync(Guid eventId, string eventName, int version, CancellationToken ct)
    {
        var subscribers = _registry.GetSubscribers(eventName, version);
        using var scope = _scopeFactory.CreateScope();
        var db = _options.ResolveDbContext!(scope.ServiceProvider);

        foreach (var subscriber in subscribers)
        {
            db.Set<IntegrationDelivery>().Add(new IntegrationDelivery
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Subscriber = subscriber,
                Attempts = 0,
                ProcessedUtc = null
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
