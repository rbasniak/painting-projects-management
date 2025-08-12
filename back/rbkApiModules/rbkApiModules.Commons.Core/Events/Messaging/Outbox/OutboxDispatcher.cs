using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace rbkApiModules.Commons.Core;

public sealed class OutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventTypeRegistry _registry;
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly OutboxOptions _options;

    public OutboxDispatcher(
        IServiceScopeFactory scopeFactory,
        IEventTypeRegistry registry,
        ILogger<OutboxDispatcher> logger,
        IOptions<OutboxOptions> options)
    {
        _scopeFactory = scopeFactory;
        _registry = registry;
        _logger = logger;
        _options = options.Value;

        if (_options.ResolveDbContext == null)
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
                using var scope = _scopeFactory.CreateScope();

                var db = _options.ResolveDbContext!(scope.ServiceProvider);

                using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

                var outboxSet = db.Set<OutboxMessage>();
                var inboxSet = db.Set<InboxMessage>();

                var batch = await outboxSet
                    .Where(x => x.ProcessedUtc == null && x.Attempts < _options.MaxAttempts)
                    .OrderBy(x => x.CreatedUtc)
                    .Take(_options.BatchSize)
                    .ToListAsync(cancellationToken);

                if (batch.Count == 0)
                {
                    await Task.Delay(_options.PollIntervalMs, cancellationToken);
                    continue;
                }

                foreach (var msg in batch)
                {
                    var sw = Stopwatch.StartNew();
                    using var scopeLog = _logger.BeginScope(new Dictionary<string, object>
                    {
                        ["EventId"] = msg.Id,
                        ["CorrelationId"] = msg.CorrelationId ?? string.Empty,
                        ["Name"] = msg.Name,
                        ["Version"] = msg.Version,
                        ["TenantId"] = msg.TenantId
                    });

                    try
                    {
                        if (!_registry.TryResolve(msg.Name, msg.Version, out var clrType))
                        {
                            _logger.LogWarning("No event type found for {Name} v{Version}", msg.Name, msg.Version);
                            msg.Attempts++;
                            await db.SaveChangesAsync(cancellationToken);
                            continue;
                        }

                        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(clrType);

                        var envelope = JsonEventSerializer.Deserialize(msg.Payload, envelopeType);

                        var handlers = ResolveHandlers(scope.ServiceProvider, clrType);
                        foreach (var handler in handlers)
                        {
                            var handlerName = handler.GetType().FullName!;

                            var already = await inboxSet.FindAsync(new object[] { msg.Id, handlerName }, cancellationToken);
                            if (already is not null) continue;

                            _logger.LogInformation("Dispatching event {Name} v{Version} to handler {Handler}", msg.Name, msg.Version, handlerName);

                            await InvokeHandler(handler, envelope, cancellationToken);

                            inboxSet.Add(new InboxMessage
                            {
                                EventId = msg.Id,
                                HandlerName = handlerName,
                                ProcessedUtc = DateTime.UtcNow,
                                Attempts = 1
                            });
                            await db.SaveChangesAsync(cancellationToken);
                        }

                        msg.ProcessedUtc = DateTime.UtcNow;
                        await db.SaveChangesAsync(cancellationToken);

                        await tx.CommitAsync(cancellationToken);

                        sw.Stop();
                        EventsMeters.OutboxMessagesProcessed.Add(1);
                        EventsMeters.OutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        EventsMeters.OutboxMessagesFailed.Add(1);
                        EventsMeters.OutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);

                        _logger.LogError(ex, "Outbox dispatch failed for {Id}", msg.Id);
                        msg.Attempts++;

                        // jittered exponential backoff based on attempts
                        var backoffMs = ComputeBackoffMs(msg.Attempts, _options.PollIntervalMs);
                        _logger.LogWarning("Scheduling retry for {Id} in ~{BackoffMs}ms (attempt {Attempts})", msg.Id, backoffMs, msg.Attempts);
                        msg.CreatedUtc = DateTime.UtcNow.AddMilliseconds(backoffMs); // move to later by adjusting order key

                        await db.SaveChangesAsync(cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // graceful shutdown
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
                catch (OperationCanceledException) { }
            }
        }

        _logger.LogInformation("OutboxDispatcher stopped");
    }

    private static int ComputeBackoffMs(int attempts, int basePollMs)
    {
        var min = Math.Min(attempts, 10); // cap exponent
        var exp = Math.Pow(2, min);
        var jitter = Random.Shared.NextDouble() * 0.25 + 0.75; // 0.75x - 1.0x
        var ms = (int)(basePollMs * exp * jitter);
        return Math.Clamp(ms, basePollMs, 60_000);
    }

    private static IEnumerable<object> ResolveHandlers(IServiceProvider sp, Type clrType)
        => sp.GetServices(typeof(IEventHandler<>).MakeGenericType(clrType))?.Cast<object>() ?? Array.Empty<object>();

    private static Task InvokeHandler(object handler, object envelope, CancellationToken ct)
    {
        var method = handler.GetType().GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance)!;
        return (Task)method.Invoke(handler, new[] { envelope, ct })!;
    }
} 