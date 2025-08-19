using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text;

namespace rbkApiModules.Commons.Core;

public class IntegrationEventPublisher : BackgroundService
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBrokerPublisher _publisher;
    private readonly IEventTypeRegistry _eventTypeRegistry;
    private readonly ILogger<IntegrationEventPublisher> _logger;
    private readonly DomainEventDispatcherOptions _options;

    public IntegrationEventPublisher(IServiceScopeFactory scopeFactory, IBrokerPublisher publisher, IEventTypeRegistry eventTypeRegistry, ILogger<IntegrationEventPublisher> logger, IOptions<DomainEventDispatcherOptions> options)
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

                // 1) QUIET "is there anything to do?" check (no telemetry)
                if (!await HasDueIntegrationAsync(db, stoppingToken))
                {
                    await Task.Delay(_options.PollIntervalMs, stoppingToken);
                    continue;
                }

                // 2) Real claim (instrumented). Only now we’ll emit spans.
                var sql = $@"SELECT * FROM ""OutboxIntegrationEvents""
                            WHERE ""ProcessedUtc"" IS NULL
                              AND (""DoNotProcessBeforeUtc"" IS NULL OR ""DoNotProcessBeforeUtc"" <= NOW())
                            ORDER BY ""CreatedUtc""
                            LIMIT {_options.BatchSize}
                            FOR UPDATE SKIP LOCKED";

                var batch = await db.Set<IntegrationOutboxMessage>().FromSqlRaw(sql).ToListAsync(stoppingToken);

                foreach (var row in batch)
                {
                    var sw = Stopwatch.StartNew();

                    ActivityLink[]? links = null;
                    if (TryBuildParent(row, out var parentCtx))
                        links = new[] { new ActivityLink(parentCtx) };

                    using var activity = EventsTracing.ActivitySource.StartActivity(
                        "integration.outbox.publish",
                        ActivityKind.Producer,
                        default(ActivityContext),                                // no parent
                        (IEnumerable<KeyValuePair<string, object?>>?)null,
                        links,
                        default);

                    activity?.SetTag("messaging.system", "rabbitmq");
                    activity?.SetTag("messaging.destination_kind", "topic");
                    activity?.SetTag("messaging.message_id", row.Id);
                    activity?.SetTag("messaging.event.name", row.Name);
                    activity?.SetTag("messaging.event.version", row.Version);

                    try
                    {
                        if (!_eventTypeRegistry.TryResolve(row.Name, row.Version, out var clrType))
                        {
                            row.Attempts++;
                            row.DoNotProcessBeforeUtc = DateTime.UtcNow.AddSeconds(30);
                            await db.SaveChangesAsync(stoppingToken);
                            continue;
                        }

                        // touch payload type (optional)
                        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(clrType);
                        JsonEventSerializer.Deserialize(row.Payload, envelopeType);

                        var topic = $"{row.Name}.v{row.Version}";

                        // Inject W3C trace headers
                        var headers = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                        if (activity is not null)
                        {
                            Propagator.Inject(
                                new PropagationContext(activity.Context, Baggage.Current),
                                headers,
                                static (h, k, v) => h[k] = Encoding.UTF8.GetBytes(v));
                        }

                        await _publisher.PublishAsync(topic, Encoding.UTF8.GetBytes(row.Payload), headers, stoppingToken);

                        row.ProcessedUtc = DateTime.UtcNow;
                        await db.SaveChangesAsync(stoppingToken);

                        sw.Stop();
                        EventsMeters.IntegrationOutboxMessagesProcessed.Add(1);
                        EventsMeters.IntegrationOutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        EventsMeters.IntegrationOutboxMessagesFailed.Add(1);
                        EventsMeters.IntegrationOutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);

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

            try { await Task.Delay(_options.PollIntervalMs, stoppingToken); }
            catch (OperationCanceledException) { }
        }
    }

    private static async Task<bool> HasDueIntegrationAsync(DbContext db, CancellationToken ct)
    {
        using var _ = SuppressInstrumentationScope.Begin();   // <- silences EF/Npgsql instrumentation here

        const string sql = @"
            SELECT 1
              FROM ""OutboxIntegrationEvents""
             WHERE ""ProcessedUtc"" IS NULL
               AND (""DoNotProcessBeforeUtc"" IS NULL OR ""DoNotProcessBeforeUtc"" <= NOW())
             LIMIT 1";

        await using var conn = db.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        if (conn.State != System.Data.ConnectionState.Open)
        {
            await conn.OpenAsync(ct);
        }

        var r = await cmd.ExecuteScalarAsync(ct);

        return r is not null;
    }

    private static bool TryBuildParent(IntegrationOutboxMessage m, out ActivityContext parent)
    {
        parent = default;
        if (string.IsNullOrWhiteSpace(m.TraceId) || string.IsNullOrWhiteSpace(m.ParentSpanId)) return false;
        try
        {
            parent = new ActivityContext(
                ActivityTraceId.CreateFromString(m.TraceId.AsSpan()),
                ActivitySpanId.CreateFromString(m.ParentSpanId.AsSpan()),
                (ActivityTraceFlags)(m.TraceFlags ?? 0),
                m.TraceState,
                isRemote: true);
            return true;
        }
        catch { return false; }
    }
}
