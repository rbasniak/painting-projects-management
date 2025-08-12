using System.Reflection;
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

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("OutboxDispatcher started");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                // 1) Get a batch (short-lived scope, no tracking)
                using (var scopeQ = _scopeFactory.CreateScope())
                {
                    var dbQ = _options.ResolveDbContext!(scopeQ.ServiceProvider);
                    var now = DateTime.UtcNow;

                    var batch = await dbQ.Set<OutboxMessage>()
                        .AsNoTracking()
                        .Where(x => x.ProcessedUtc == null
                                 && (x.DoNotProcessBeforeUtc == null || x.DoNotProcessBeforeUtc <= now)
                                 && x.Attempts < _options.MaxAttempts)
                        .OrderBy(x => x.CreatedUtc)
                        .Take(_options.BatchSize)
                        .Select(x => x.Id) // get keys only
                        .ToListAsync(ct);

                    if (batch.Count == 0)
                    {
                        await Task.Delay(_options.PollIntervalMs, ct);
                        continue;
                    }

                    // 2) Process each message in its own scope/transaction
                    foreach (var msgId in batch)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var db = _options.ResolveDbContext!(scope.ServiceProvider);

                        // (re)load the message in this context
                        var msg = await db.Set<OutboxMessage>().FirstOrDefaultAsync(x => x.Id == msgId, ct);
                        if (msg is null) continue; // deleted / raced
                        if (msg.ProcessedUtc != null) continue; // already done
                        if (msg.DoNotProcessBeforeUtc.HasValue && msg.DoNotProcessBeforeUtc > DateTime.UtcNow) continue;

                        using var tx = await db.Database.BeginTransactionAsync(ct);

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
                                msg.DoNotProcessBeforeUtc = DateTime.UtcNow.Add(ComputeBackoff(msg.Attempts));
                                await db.SaveChangesAsync(ct);
                                await tx.CommitAsync(ct);
                                continue;
                            }

                            var envelopeType = typeof(EventEnvelope<>).MakeGenericType(clrType);
                            var envelope = JsonEventSerializer.Deserialize(msg.Payload, envelopeType);

                            var inboxSet = db.Set<InboxMessage>();
                            var handlers = ResolveHandlers(scope.ServiceProvider, clrType);

                            foreach (var handler in handlers)
                            {
                                var handlerName = handler.GetType().FullName!;
                                var already = await inboxSet.FindAsync(new object[] { msg.Id, handlerName }, ct);
                                if (already is not null) continue;

                                _logger.LogInformation("Dispatching {Name} v{Version} to {Handler}", msg.Name, msg.Version, handlerName);

                                await InvokeHandler(handler, envelope, ct);

                                inboxSet.Add(new InboxMessage
                                {
                                    EventId = msg.Id,
                                    HandlerName = handlerName,
                                    ProcessedUtc = DateTime.UtcNow,
                                    Attempts = 1
                                });
                            }

                            // mark processed after all handlers succeed
                            msg.ProcessedUtc = DateTime.UtcNow;

                            await db.SaveChangesAsync(ct);
                            await tx.CommitAsync(ct);

                            sw.Stop();
                            EventsMeters.OutboxMessagesProcessed.Add(1);
                            EventsMeters.OutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                        }
                        catch (Exception ex)
                        {
                            // rollback tx and backoff
                            try { await tx.RollbackAsync(ct); } catch { /* ignore */ }

                            var attempts = msg.Attempts + 1;
                            msg.Attempts = attempts;
                            msg.DoNotProcessBeforeUtc = DateTime.UtcNow.Add(ComputeBackoff(attempts));
                            await db.SaveChangesAsync(ct);

                            sw.Stop();
                            EventsMeters.OutboxMessagesFailed.Add(1);
                            EventsMeters.OutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                            _logger.LogError(ex, "Outbox dispatch failed for {Id}", msg.Id);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { /* shutdown */ }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxDispatcher loop error");
            }

            if (!ct.IsCancellationRequested)
            {
                try { await Task.Delay(_options.PollIntervalMs, ct); }
                catch (OperationCanceledException) { }
            }
        }

        _logger.LogInformation("OutboxDispatcher stopped");
    }

    private static TimeSpan ComputeBackoff(int attempts)
    {
        var baseSeconds = Math.Min(300, (int)Math.Pow(2, Math.Min(10, attempts)));
        var jitter = Random.Shared.Next(0, 1000);
        return TimeSpan.FromSeconds(baseSeconds).Add(TimeSpan.FromMilliseconds(jitter));
    }

    private static IEnumerable<object> ResolveHandlers(IServiceProvider sp, Type clrType)
        => sp.GetServices(typeof(IEventHandler<>).MakeGenericType(clrType))?.Cast<object>() ?? Array.Empty<object>();

    private static Task InvokeHandler(object handler, object envelope, CancellationToken ct)
    {
        var method = handler.GetType().GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance)!;
        return (Task)method.Invoke(handler, new[] { envelope, ct })!;
    }
} 