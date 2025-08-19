// TODO: DONE, REVIEWED

// TODO: implement a dead letter queue for messages that failed too many times (table or status in current table?)

// DOCS: Keep handlers DB-only. No HTTP, queues, or other databases inside the transaction.
//       If a handler must trigger external effects, persist an outbox record for that effect instead of calling the external system inline.
//       Watch transaction length and lock contention. Consider smaller batches, reasonable command timeout, and clear guidance that handlers must be fast and idempotent.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace rbkApiModules.Commons.Core;

public sealed class DomainEventDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventTypeRegistry _eventTypeRegistry;
    private readonly ILogger<DomainEventDispatcher> _logger;
    private readonly DomainEventDispatcherOptions _options;

    private static readonly ConcurrentDictionary<Type, Func<object, object, CancellationToken, Task>> _dispatchers = new();

    public DomainEventDispatcher(
        IServiceScopeFactory scopeFactory,
        IEventTypeRegistry registry,
        ILogger<DomainEventDispatcher> logger,
        IOptions<DomainEventDispatcherOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(logger);

        _scopeFactory = scopeFactory;
        _eventTypeRegistry = registry;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OutboxDispatcher started");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Guid[] batch = [];

                // Get a batch of messages to process (short-lived scope, no tracking)
                // Keep things clean, not cached entities because of long-running scope
                using (var scope = _scopeFactory.CreateScope())
                {
                    var messagingContext = _options.ResolveSilentDbContext!(scope.ServiceProvider);

                    batch = await GetMessagesToProcessAsync(messagingContext, _options.MaxAttempts, _options.BatchSize, cancellationToken);

                    if (batch.Length == 0)
                    {
                        try
                        {
                            await Task.Delay(_options.PollIntervalMs, cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                        }

                        continue; // no messages due to processing, skip this tick without emitting logs or telemetry
                    }
                }

                var outerLogExtraData = new Dictionary<string, object>
                {
                    ["DomainOutboxDispatcherInstanceId"] = Guid.CreateVersion7().ToString("N"),
                };

                using var outerLogScope = _logger.BeginScope(outerLogExtraData);

                _logger.LogInformation("Found {Count} domain messages to dispatch", batch.Length);

                // Process each message in its own scope/transaction
                foreach (var messageId in batch)
                {
                    _logger.LogInformation("Processing message {Id}", messageId);

                    using var scope = _scopeFactory.CreateScope();

                    var context = _options.ResolveDbContext(scope.ServiceProvider);

                    var sw = Stopwatch.StartNew();

                    await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

                    // Reload the full message in this context
                    // This is important to ensure we have the latest state and can handle concurrency correctly
                    var message = await context.OutboxDomainMessages.FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

                    if (message is null)
                    {
                        continue; // deleted 
                    }

                    var logExtraData = new Dictionary<string, object>
                    {
                        ["EventId"] = message.Id,
                        ["CorrelationId"] = message.CorrelationId ?? string.Empty,
                        ["Name"] = message.Name,
                        ["Version"] = message.Version,
                        ["Username"] = message.Username,
                        ["TenantId"] = message.TenantId
                    };

                    using var scopeLog = _logger.BeginScope(logExtraData);

                    if (message.ProcessedUtc != null)
                    {
                        _logger.LogDebug("Message {Id} already processed at {ProcessedUtc}, skipping", message.Id, message.ProcessedUtc);
                        
                        continue; // already done or raced
                    }

                    if (message.DoNotProcessBeforeUtc.HasValue && message.DoNotProcessBeforeUtc > DateTime.UtcNow)
                    {
                        _logger.LogWarning("Message {Id} has back off and is not due yet, skipping until {Date}", message.Id, message.DoNotProcessBeforeUtc);
                        
                        continue;
                    }


                    var hasUpstream = TryBuildUpstreamActivityContext(message, out var upstreamContext);

                    IEnumerable<ActivityLink>? links = hasUpstream ? [new ActivityLink(upstreamContext)] : null;

                    using var dispatchActivity =
                        EventsTracing.ActivitySource.StartActivity(
                            "outbox.dispatch",
                            ActivityKind.Consumer,
                            default(ActivityContext), // no parent, otherwise it will grow forever until the next http request, use linked activities instead
                            null,
                            links,
                            default);

                    if (dispatchActivity is not null)
                    {
                        dispatchActivity.SetTag("messaging.system", "outbox");
                        dispatchActivity.SetTag("messaging.domain-event.id", message.Id);
                        dispatchActivity.SetTag("messaging.domain-event.name", message.Name);
                        dispatchActivity.SetTag("messaging.domain-event.version", message.Version);

                        // For UI's that don't support showing the linked activities, we need to manually
                        // query the other spans when debugging or investigating issues
                        // Adding upstream trace and span IDs as tags to help with that
                        if (hasUpstream)
                        {
                            dispatchActivity.SetTag("upstream.trace_id", upstreamContext.TraceId.ToString());
                            dispatchActivity.SetTag("upstream.span_id", upstreamContext.SpanId.ToString());
                        }

                        dispatchActivity.SetTag("correlation.id", message.CorrelationId ?? "");
                    }

                    try
                    {
                        if (!_eventTypeRegistry.TryResolve(message.Name, message.Version, out var domainEventType))
                        {
                            _logger.LogError("No event type found for {Name} v{Version}", message.Name, message.Version);

                            dispatchActivity?.AddEvent(new ActivityEvent("handler.not-found",
                                   tags: new ActivityTagsCollection {
                                       { "domain-event-name", message.Name },
                                       { "domain-event-version", message.Version},
                                       { "reason", "handler-not-found" }
                                   }));

                            message.Backoff();

                            await context.SaveChangesAsync(cancellationToken);
                            
                            await transaction.CommitAsync(cancellationToken);
                            
                            continue;
                        }

                        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(domainEventType);

                        var envelope = JsonEventSerializer.Deserialize(message.Payload, envelopeType);

                        var handlers = ResolveHandlers(scope.ServiceProvider, domainEventType);

                        foreach (var handler in handlers)
                        {
                            var handlerName = handler.GetType().FullName!;

                            // Check if this handler has already processed the message at some point
                            var processedMessage = await context.InboxMessages.FindAsync([ message.Id, handlerName ], cancellationToken);

                            // Just in case, but should not happen because if one handler fails, we fail all together
                            if (processedMessage is not null)
                            {
                                dispatchActivity?.AddEvent(new ActivityEvent("handler.skipped",
                                   tags: new ActivityTagsCollection {
                                       { "handler", handlerName }, 
                                       { "reason", "inbox-duplicate" }
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

                            // KNOWN ISSUE: in a scenario where multiple dispatchers are running,
                            // it's possible that one of them processes the message before this one
                            // and the other one will try to insert the same InboxMessage,
                            // causing a unique constraint violation.
                            context.InboxMessages.Add(new InboxMessage
                            {
                                EventId = message.Id,
                                HandlerName = handlerName,
                                ProcessedUtc = DateTime.UtcNow,
                                Attempts = 1
                            });
                        }

                        // mark as processed only after all handlers succeed
                        message.ProcessedUtc = DateTime.UtcNow;

                        await context.SaveChangesAsync(cancellationToken);

                        await transaction.CommitAsync(cancellationToken);

                        EventsMeters.DomainOutboxMessagesProcessed.Add(1);
                        EventsMeters.DomainOutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Outbox dispatch failed for {Id}", message.Id);

                        // rollback transacation and backoff
                        try 
                        { 
                            await transaction.RollbackAsync(cancellationToken); 
                        } 
                        catch (Exception transactionException)
                        { 
                            _logger.LogWarning(transactionException, "Failed to rollback transaction for message {Id}", message.Id);
                        }

                        dispatchActivity?.AddEvent(new ActivityEvent("handler.error", tags: new ActivityTagsCollection 
                        { 
                            { "exception.message", ex.Message },
                            { "exception.details", ex.ToBetterString() },
                        }));

                        message.Backoff();

                        await context.SaveChangesAsync(cancellationToken);

                        EventsMeters.DomainOutboxMessagesFailed.Add(1);
                        EventsMeters.DomainOutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                    }
                    finally
                    {
                        sw.Stop();
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

    private static bool TryBuildUpstreamActivityContext(DomainOutboxMessages message, out ActivityContext parent)
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


    private IEnumerable<object> ResolveHandlers(IServiceProvider sp, Type clrType)
        => sp.GetServices(typeof(IEventHandler<>).MakeGenericType(clrType))?.Cast<object>() ?? Array.Empty<object>();

    private static Task InvokeHandler(object handler, object envelope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(envelope);

        var handlerType = handler.GetType();
        var invoker = _dispatchers.GetOrAdd(handlerType, x =>
        {
            var method = x.GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance);

            if (method is null)
            {
                throw new InvalidOperationException($"Handler {handler.GetType().FullName} does not have a public Handle method.");
            }

            return (handler, envelope, cancellationToken) => (Task)method.Invoke(handler, [ envelope, cancellationToken ])!;
        });

        return invoker(handler, envelope, cancellationToken);
    }


    private static async Task<Guid[]> GetMessagesToProcessAsync(MessagingDbContext context, int maxAttempts, int batchSize, CancellationToken cancellationToken)
    {
        using var _ = SuppressInstrumentationScope.Begin(); // no telemetry, avoid flooding it with idle loops data

        var now = DateTime.UtcNow; // close enough for "due" check

        // KNOWN ISSUE: in a scenario with multiple dispatchers running at same time, 
        // it's possible that more than one dispatcher will pick the same batch of messages
        // If this scenario can happen, we might need to add a distributed lock in Postgres but 
        // for that we need to ditch EF and use raw SQL queries with advisory locks
        var batch = await context.Set<DomainOutboxMessages>()
            .AsNoTracking()
            .Where(x => x.ProcessedUtc == null)
            .Where(x => x.DoNotProcessBeforeUtc == null || x.DoNotProcessBeforeUtc <= now)
            .Where(x => x.Attempts < maxAttempts)
            .OrderBy(x => x.CreatedUtc)
            .Take(batchSize)
            .Select(x => x.Id) // get keys only, because messages are reloaded in the processing scope
            .ToArrayAsync(cancellationToken);

        return batch;
    }


}