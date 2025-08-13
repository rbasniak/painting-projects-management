using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Background service responsible for processing integration deliveries.
/// </summary>
public sealed class IntegrationDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventTypeRegistry _eventRegistry;
    private readonly ILogger<IntegrationDispatcher> _logger;
    private readonly OutboxOptions _options;

    public IntegrationDispatcher(IServiceScopeFactory scopeFactory,
        IEventTypeRegistry eventRegistry,
        ILogger<IntegrationDispatcher> logger,
        IOptions<OutboxOptions> options)
    {
        _scopeFactory = scopeFactory;
        _eventRegistry = eventRegistry;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Guid[] deliveries;
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = _options.ResolveDbContext!(scope.ServiceProvider);
                deliveries = await db.Set<IntegrationDelivery>()
                    .AsNoTracking()
                    .Where(x => x.ProcessedUtc == null && x.Attempts < _options.MaxAttempts)
                    .OrderBy(x => x.Id)
                    .Select(x => x.Id)
                    .Take(_options.BatchSize)
                    .ToArrayAsync(stoppingToken);
            }

            foreach (var deliveryId in deliveries)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = _options.ResolveDbContext!(scope.ServiceProvider);
                var delivery = await db.Set<IntegrationDelivery>().FirstOrDefaultAsync(x => x.Id == deliveryId, stoppingToken);
                if (delivery == null || delivery.ProcessedUtc != null)
                {
                    continue;
                }

                var evt = await db.Set<OutboxIntegrationEvent>().FirstOrDefaultAsync(x => x.Id == delivery.EventId, stoppingToken);
                if (evt == null)
                {
                    delivery.ProcessedUtc = DateTime.UtcNow;
                    await db.SaveChangesAsync(stoppingToken);
                    continue;
                }

                var eventType = _eventRegistry.GetEventType(evt.Name, evt.Version);
                if (eventType == null)
                {
                    delivery.ProcessedUtc = DateTime.UtcNow;
                    await db.SaveChangesAsync(stoppingToken);
                    continue;
                }

                var envelope = JsonEventSerializer.Deserialize(evt.Payload, eventType);
                var handlerType = Type.GetType(delivery.Subscriber);
                if (handlerType == null)
                {
                    delivery.ProcessedUtc = DateTime.UtcNow;
                    await db.SaveChangesAsync(stoppingToken);
                    continue;
                }

                try
                {
                    var handler = scope.ServiceProvider.GetService(handlerType);
                    if (handler != null)
                    {
                        var method = handlerType.GetMethod("Handle");
                        var task = (Task?)method?.Invoke(handler, new[] { envelope, stoppingToken });
                        if (task != null)
                        {
                            await task.ConfigureAwait(false);
                        }
                    }

                    delivery.ProcessedUtc = DateTime.UtcNow;
                    await db.SaveChangesAsync(stoppingToken);

                    var remaining = await db.Set<IntegrationDelivery>()
                        .AnyAsync(x => x.EventId == delivery.EventId && x.ProcessedUtc == null, stoppingToken);
                    if (!remaining)
                    {
                        evt.ProcessedUtc = DateTime.UtcNow;
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Integration delivery failed");
                    delivery.Attempts++;
                    await db.SaveChangesAsync(stoppingToken);
                }
            }

            await Task.Delay(_options.PollIntervalMs, stoppingToken);
        }
    }
}
