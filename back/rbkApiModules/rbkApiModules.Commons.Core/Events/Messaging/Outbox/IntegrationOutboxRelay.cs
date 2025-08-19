using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace rbkApiModules.Commons.Core;

public class IntegrationOutboxRelay : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBrokerPublisher _publisher;
    private readonly IEventTypeRegistry _eventTypeRegistry;
    private readonly ILogger<IntegrationOutboxRelay> _logger;
    private readonly OutboxOptions _options;

    public IntegrationOutboxRelay(IServiceScopeFactory scopeFactory, IBrokerPublisher publisher, IEventTypeRegistry eventTypeRegistry, ILogger<IntegrationOutboxRelay> logger, IOptions<OutboxOptions> options)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _eventTypeRegistry = eventTypeRegistry;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = _options.ResolveDbContext!(scope.ServiceProvider);
                var sql = $@"SELECT * FROM ""OutboxIntegrationEvents"" WHERE ""ProcessedUtc"" IS NULL AND (""DoNotProcessBeforeUtc"" IS NULL OR ""DoNotProcessBeforeUtc"" <= NOW()) ORDER BY ""CreatedUtc"" LIMIT {_options.BatchSize} FOR UPDATE SKIP LOCKED";
                var batch = await db.Set<OutboxIntegrationEvent>()
                    .FromSqlRaw(sql)
                    .ToListAsync(stoppingToken);

                foreach (var row in batch)
                {
                    var sw = Stopwatch.StartNew();
                    using var activity = EventsTracing.ActivitySource.StartActivity("integration.outbox.publish", ActivityKind.Producer);
                    if (activity is not null)
                    {
                        activity.SetTag("messaging.event.id", row.Id);
                        activity.SetTag("messaging.event.name", row.Name);
                        activity.SetTag("messaging.event.version", row.Version);
                    }

                    try
                    {
                        if (!_eventTypeRegistry.TryResolve(row.Name, row.Version, out var clrType))
                        {
                            row.Attempts++;
                            row.DoNotProcessBeforeUtc = DateTime.UtcNow.AddSeconds(30);
                            await db.SaveChangesAsync(stoppingToken);
                            continue;
                        }

                        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(clrType);
                        JsonEventSerializer.Deserialize(row.Payload, envelopeType);

                        var topic = $"{row.Name}.v{row.Version}";
                        await _publisher.PublishAsync(topic, Encoding.UTF8.GetBytes(row.Payload), stoppingToken);
                        row.ProcessedUtc = DateTime.UtcNow;
                        await db.SaveChangesAsync(stoppingToken);

                        sw.Stop();
                        EventsMeters.OutboxMessagesProcessed.Add(1);
                        EventsMeters.OutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        EventsMeters.OutboxMessagesFailed.Add(1);
                        EventsMeters.OutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);

                        _logger.LogError(ex, "Failed to relay integration event {EventId}", row.Id);
                        row.Attempts++;
                        row.DoNotProcessBeforeUtc = DateTime.UtcNow.AddSeconds(30);
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IntegrationOutboxRelay loop error");
            }

            try
            {
                await Task.Delay(_options.PollIntervalMs, stoppingToken);
            }
            catch (OperationCanceledException) { }
        }
    }
}
