using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Background service responsible for delivering integration events to
/// their subscribers using the IntegrationDeliveries table.
/// </summary>
public sealed class IntegrationOutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventTypeRegistry _eventTypeRegistry;
    private readonly ILogger<IntegrationOutboxDispatcher> _logger;
    private readonly OutboxOptions _options;

    public IntegrationOutboxDispatcher(
        IServiceScopeFactory scopeFactory,
        IEventTypeRegistry eventTypeRegistry,
        ILogger<IntegrationOutboxDispatcher> logger,
        IOptions<OutboxOptions> options)
    {
        _scopeFactory = scopeFactory;
        _eventTypeRegistry = eventTypeRegistry;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IntegrationDispatcher started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                (Guid EventId, string Subscriber)[] batch = [];
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = _options.ResolveDbContext!(scope.ServiceProvider);
                    var now = DateTime.UtcNow;
                    batch = await db.Set<IntegrationDelivery>()
                        .AsNoTracking()
                        .Where(x => x.ProcessedUtc == null
                                 && (x.DoNotProcessBeforeUtc == null || x.DoNotProcessBeforeUtc <= now)
                                 && x.Attempts < _options.MaxAttempts)
                        .OrderBy(x => x.Event.CreatedUtc)
                        .Take(_options.BatchSize)
                        .Select(x => new ValueTuple<Guid, string>(x.EventId, x.Subscriber))
                        .ToArrayAsync(stoppingToken);
                }

                foreach (var item in batch)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = _options.ResolveDbContext!(scope.ServiceProvider);
                    var delivery = await db.Set<IntegrationDelivery>()
                        .Include(d => d.Event)
                        .FirstOrDefaultAsync(d => d.EventId == item.EventId && d.Subscriber == item.Subscriber, stoppingToken);

                    if (delivery == null || delivery.ProcessedUtc != null)
                    {
                        continue;
                    }
                    if (delivery.DoNotProcessBeforeUtc.HasValue && delivery.DoNotProcessBeforeUtc > DateTime.UtcNow)
                    {
                        continue;
                    }

                    using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        if (!_eventTypeRegistry.TryResolve(delivery.Event.Name, delivery.Event.Version, out var clrType))
                        {
                            delivery.Attempts++;
                            delivery.DoNotProcessBeforeUtc = DateTime.UtcNow.AddSeconds(30);
                            await db.SaveChangesAsync(stoppingToken);
                            await transaction.CommitAsync(stoppingToken);
                            continue;
                        }

                        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(clrType);
                        var envelope = JsonEventSerializer.Deserialize(delivery.Event.Payload, envelopeType);
                        var handlerType = Type.GetType(delivery.Subscriber);
                        if (handlerType == null)
                        {
                            delivery.Attempts++;
                            delivery.DoNotProcessBeforeUtc = DateTime.UtcNow.AddSeconds(30);
                            await db.SaveChangesAsync(stoppingToken);
                            await transaction.CommitAsync(stoppingToken);
                            continue;
                        }

                        var handler = scope.ServiceProvider.GetRequiredService(handlerType);
                        var method = handlerType.GetMethod("Handle");
                        await (Task)method!.Invoke(handler, new object?[] { envelope!, stoppingToken })!;

                        delivery.ProcessedUtc = DateTime.UtcNow;
                        await db.SaveChangesAsync(stoppingToken);

                        var remaining = await db.Set<IntegrationDelivery>()
                            .Where(d => d.EventId == delivery.EventId && d.ProcessedUtc == null)
                            .AnyAsync(stoppingToken);
                        if (!remaining)
                        {
                            delivery.Event.ProcessedUtc = DateTime.UtcNow;
                            await db.SaveChangesAsync(stoppingToken);
                        }

                        await transaction.CommitAsync(stoppingToken);
                        sw.Stop();
                        EventsMeters.OutboxMessagesProcessed.Add(1);
                        EventsMeters.OutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            await transaction.RollbackAsync(stoppingToken);
                        }
                        catch { }
                        delivery.Attempts++;
                        delivery.DoNotProcessBeforeUtc = DateTime.UtcNow.AddSeconds(30);
                        await db.SaveChangesAsync(stoppingToken);
                        sw.Stop();
                        EventsMeters.OutboxMessagesFailed.Add(1);
                        EventsMeters.OutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                        _logger.LogError(ex, "Integration delivery failed for {EventId} -> {Subscriber}", delivery.EventId, delivery.Subscriber);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IntegrationDispatcher loop error");
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_options.PollIntervalMs, stoppingToken);
                }
                catch (OperationCanceledException) { }
            }
        }
    }
}
