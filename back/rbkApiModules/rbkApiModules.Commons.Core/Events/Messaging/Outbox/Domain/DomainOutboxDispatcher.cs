using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace rbkApiModules.Commons.Core;

public sealed class DomainOutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventTypeRegistry _eventTypeRegistry;
    private readonly ILogger<DomainOutboxDispatcher> _logger;
    private readonly IDbContextFactory<MessagingDbContext> _factory;
    private readonly OutboxOptions _options;

    public DomainOutboxDispatcher(
        IServiceScopeFactory scopeFactory,
        IEventTypeRegistry registry,
        ILogger<DomainOutboxDispatcher> logger,
        IOptions<OutboxOptions> options)
    {
        _scopeFactory = scopeFactory;
        _eventTypeRegistry = registry;
        _logger = logger;
        _options = options.Value;

        if (_options.ResolveSilentDbContext == null)
        {
            throw new InvalidOperationException("OutboxOptions.ResolveDbContext must be configured to resolve the application's DbContext.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OutboxDispatcher started");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Guid[] batch = [];

                // 1) Get a batch (short-lived scope, no tracking)
                using (var scope = _scopeFactory.CreateScope())
                {
                    // TODO: do a cheap check and if there is anything fetch, or just always fetch?
                    var context = _options.ResolveSilentDbContext!(scope.ServiceProvider);
                    batch = await GetMessagesToProcessAsync(context, _options.MaxAttempts, _options.BatchSize, cancellationToken);

                    if (batch.Length == 0)
                    {
                        try
                        {
                            await Task.Delay(_options.PollIntervalMs, cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                        }

                        continue; // nothing due → skip this tick without emitting telemetry
                    }

                    _logger.LogInformation("Found {Count} domain messages to dispatch", batch.Length);
                }

                // 2) Process each message in its own scope/transaction
                foreach (var messageId in batch)
                {
                    _logger.LogInformation("Processing message {Id}", messageId);

                    using var scope = _scopeFactory.CreateScope();
                    var context = _options.ResolveDbContext!(scope.ServiceProvider);

                    var sw = Stopwatch.StartNew();

                    // (re)load the message in this context
                    var message = await context.OutboxDomainMessages.FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

                    using var scopeLog = _logger.BeginScope(new Dictionary<string, object>
                    {
                        ["EventId"] = message.Id,
                        ["CorrelationId"] = message.CorrelationId ?? string.Empty,
                        ["Name"] = message.Name,
                        ["Version"] = message.Version,
                        ["Username"] = message.Username,
                        ["TenantId"] = message.TenantId
                    });

                    if (message is null)
                    {
                        continue; // deleted / raced
                    }

                    if (message.ProcessedUtc != null)
                    {
                        continue; // already done
                    }

                    if (message.DoNotProcessBeforeUtc.HasValue && message.DoNotProcessBeforeUtc > DateTime.UtcNow)
                    {
                        continue;
                    }

                    using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

                    IEnumerable<ActivityLink>? links = null;
                    if (TryBuildParent(message, out var upstreamContext))
                    {
                        links = new[] { new ActivityLink(upstreamContext) };
                    }

                    using var dispatchActivity =
                        EventsTracing.ActivitySource.StartActivity(
                            "outbox.dispatch",
                            ActivityKind.Consumer,
                            default(ActivityContext),                        // no parent
                            (IEnumerable<KeyValuePair<string, object?>>?)null,
                            links,
                            default);

                    if (dispatchActivity is not null)
                    {
                        dispatchActivity.SetTag("messaging.system", "outbox");
                        dispatchActivity.SetTag("messaging.event.id", message.Id);
                        dispatchActivity.SetTag("messaging.event.name", message.Name);
                        dispatchActivity.SetTag("messaging.event.version", message.Version);

                        // For UI's that don't support showing the linked activities, we need to manually query the other spans
                        dispatchActivity.SetTag("upstream.trace_id", upstreamContext.TraceId.ToString());
                        dispatchActivity.SetTag("upstream.span_id", upstreamContext.SpanId.ToString());

                        dispatchActivity.SetTag("correlation.id", message.CorrelationId ?? "");
                    }

                    try
                    {
                        if (!_eventTypeRegistry.TryResolve(message.Name, message.Version, out var clrType))
                        {
                            _logger.LogWarning("No event type found for {Name} v{Version}", message.Name, message.Version);

                            message.Attempts++;
                            message.DoNotProcessBeforeUtc = DateTime.UtcNow.Add(ComputeBackoff(message.Attempts));

                            await context.SaveChangesAsync(cancellationToken);
                            
                            await transaction.CommitAsync(cancellationToken);
                            
                            continue;
                        }

                        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(clrType);

                        var envelope = JsonEventSerializer.Deserialize(message.Payload, envelopeType);

                        var handlers = ResolveHandlers(scope.ServiceProvider, clrType);

                        foreach (var handler in handlers)
                        {
                            var handlerName = handler.GetType().FullName!;

                            var processedMessage = await context.InboxMessages.FindAsync(new object[] { message.Id, handlerName }, cancellationToken);

                            if (processedMessage is not null)
                            {
                                dispatchActivity?.AddEvent(new ActivityEvent("handler.skipped",
                                   tags: new ActivityTagsCollection {
                                        { "handler", handlerName }, { "reason", "inbox-duplicate" }
                                   }));

                                continue;
                            }

                            _logger.LogInformation("Dispatching {Name} v{Version} to {Handler}", message.Name, message.Version, handlerName);

                            using var handlerActivity = EventsTracing.ActivitySource.StartActivity(
                                "domain-event.handler", ActivityKind.Internal, dispatchActivity?.Context ?? default);

                            handlerActivity?.SetTag("messaging.domain-event.handler", handlerName);
                            handlerActivity?.SetTag("messaging.domain-event.name", message.Name);
                            handlerActivity?.SetTag("messaging.domain-event.version", message.Version);

                            await InvokeHandler(handler, envelope, cancellationToken);

                            context.InboxMessages.Add(new InboxMessage
                            {
                                EventId = message.Id,
                                HandlerName = handlerName,
                                ProcessedUtc = DateTime.UtcNow,
                                Attempts = 1
                            });
                        }

                        // mark processed after all handlers succeed
                        message.ProcessedUtc = DateTime.UtcNow;

                        await context.SaveChangesAsync(cancellationToken);

                        await transaction.CommitAsync(cancellationToken);

                        sw.Stop();
                        
                        EventsMeters.DomainOutboxMessagesProcessed.Add(1);
                        EventsMeters.DomainOutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        // rollback transacation and backoff
                        try 
                        { 
                            await transaction.RollbackAsync(cancellationToken); 
                        } 
                        catch 
                        { 
                            //TODO: ask why it is being ignored
                            /* ignore */ 
                        }

                        _logger.LogError(ex, "Outbox dispatch failed for {Id}", message.Id);

                        dispatchActivity?.AddEvent(new ActivityEvent("handler.error", tags: new ActivityTagsCollection 
                        { 
                            { "exception.message", ex.Message },
                            { "exception.details", ex.ToBetterString() },
                        }));

                        var attempts = message.Attempts + 1;
                        message.Attempts = attempts;
                        message.DoNotProcessBeforeUtc = DateTime.UtcNow.Add(ComputeBackoff(attempts));

                        await context.SaveChangesAsync(cancellationToken);

                        sw.Stop();

                        EventsMeters.DomainOutboxMessagesFailed.Add(1);
                        EventsMeters.DomainOutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                    }
                }
            }
            catch (OperationCanceledException) 
            {
                /* application shutdown */ 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxDispatcher loop error");
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                try 
                { 
                    await Task.Delay(_options.PollIntervalMs, cancellationToken); 
                }
                catch (OperationCanceledException) 
                {
                    /* application shutdown */
                }
            }
        }

        _logger.LogInformation("OutboxDispatcher stopped");
    }

    private static TimeSpan ComputeBackoff(int attempts)
    {
        var baseSeconds = Math.Min(300, (int)Math.Pow(2, Math.Min(10, attempts)));

        var jitter = Random.Shared.Next(0, 1000);
        
        var backoff = TimeSpan.FromSeconds(baseSeconds).Add(TimeSpan.FromMilliseconds(jitter));

        return backoff;
    }

    private static bool TryBuildParent(OutboxDomainMessage message, out ActivityContext parent)
    {
        parent = default;

        if (string.IsNullOrWhiteSpace(message.TraceId) || string.IsNullOrWhiteSpace(message.ParentSpanId) ||
            message.TraceId == "00000000000000000000000000000000" || message.ParentSpanId == "0000000000000000")
        {
            return false;
        }

        try
        {
            var traceId = ActivityTraceId.CreateFromString(message.TraceId.AsSpan());
            var spanId = ActivitySpanId.CreateFromString(message.ParentSpanId.AsSpan());

            parent = new ActivityContext(
                traceId,
                spanId,
                (ActivityTraceFlags)(message.TraceFlags ?? 0),
                message.TraceState,
                isRemote: true);

            return true;
        }
        catch
        {
            parent = default;
            return false;
        }
    }


    private static IEnumerable<object> ResolveHandlers(IServiceProvider sp, Type clrType)
        => sp.GetServices(typeof(IEventHandler<>).MakeGenericType(clrType))?.Cast<object>() ?? Array.Empty<object>();

    private static Task InvokeHandler(object handler, object envelope, CancellationToken ct)
    {
        var method = handler.GetType().GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance)!;

        return (Task)method.Invoke(handler, new[] { envelope, ct })!;
    }


    private static async Task<Guid[]> GetMessagesToProcessAsync(MessagingDbContext context, int maxAttempts, int batchSize, CancellationToken cancellationToken)
    {
        using var _ = SuppressInstrumentationScope.Begin(); // no telemetry

        var now = DateTime.UtcNow; // close enough for “due” check

        var batch = await context.Set<OutboxDomainMessage>()
            .AsNoTracking()
            .Where(x => x.ProcessedUtc == null
                     && (x.DoNotProcessBeforeUtc == null || x.DoNotProcessBeforeUtc <= now)
                     && x.Attempts < maxAttempts)
            .OrderBy(x => x.CreatedUtc)
            .Take(batchSize)
            .Select(x => x.Id) // get keys only
            .ToArrayAsync(cancellationToken);

        return batch;
    }

    // TODO: Consider cheaper version with minimal code (no LINQ pipeline, no EF interceptors, no change tracker)
    //private static async Task<bool> HasDueDomainAsync(DbContext db, int maxAttempts, CancellationToken ct)
    //{
    //    using var _ = SuppressInstrumentationScope.Begin(); // <- mute telemetry here

    //    const string sql = @"
    //    SELECT 1
    //      FROM ""OutboxDomainMessages""
    //     WHERE ""ProcessedUtc"" IS NULL
    //       AND (""DoNotProcessBeforeUtc"" IS NULL OR ""DoNotProcessBeforeUtc"" <= NOW())
    //       AND ""Attempts"" < @max
    //     LIMIT 1";

    //    await using var conn = db.Database.GetDbConnection();
    //    await conn.OpenAsync(ct);
    //    await using var cmd = conn.CreateCommand();
    //    cmd.CommandText = sql;

    //    var p = cmd.CreateParameter();
    //    p.ParameterName = "@max";
    //    p.Value = maxAttempts;
    //    cmd.Parameters.Add(p);

    //    var r = await cmd.ExecuteScalarAsync(ct);
    //    return r is not null;
    //}
}