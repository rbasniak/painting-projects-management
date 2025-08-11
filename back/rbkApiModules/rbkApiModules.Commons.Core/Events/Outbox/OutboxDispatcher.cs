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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxDispatcher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = _options.ResolveDbContext!(scope.ServiceProvider);
                var outboxSet = db.Set<OutboxMessage>();
                var inboxSet = db.Set<InboxMessage>();

                var batch = await outboxSet
                    .Where(x => x.ProcessedUtc == null && x.Attempts < _options.MaxAttempts)
                    .OrderBy(x => x.CreatedUtc)
                    .Take(_options.BatchSize)
                    .ToListAsync(stoppingToken);

                if (batch.Count == 0)
                {
                    await Task.Delay(_options.PollIntervalMs, stoppingToken);
                    continue;
                }

                foreach (var msg in batch)
                {
                    try
                    {
                        if (!_registry.TryResolve(msg.Name, msg.Version, out var clrType))
                        {
                            _logger.LogWarning("No event type found for {Name} v{Version}", msg.Name, msg.Version);
                            msg.Attempts++;
                            await db.SaveChangesAsync(stoppingToken);
                            continue;
                        }

                        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(clrType);
                        var envelope = JsonSerializer.Deserialize(msg.Payload, envelopeType, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

                        var handlers = ResolveHandlers(scope.ServiceProvider, clrType);
                        foreach (var handler in handlers)
                        {
                            var handlerName = handler.GetType().FullName!;

                            var already = await inboxSet.FindAsync(new object[] { msg.Id, handlerName }, stoppingToken);
                            if (already is not null) continue;

                            await InvokeHandler(handler, envelope, stoppingToken);

                            inboxSet.Add(new InboxMessage
                            {
                                EventId = msg.Id,
                                HandlerName = handlerName,
                                ProcessedUtc = DateTime.UtcNow,
                                Attempts = 1
                            });
                            await db.SaveChangesAsync(stoppingToken);
                        }

                        msg.ProcessedUtc = DateTime.UtcNow;
                        await db.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Outbox dispatch failed for {Id}", msg.Id);
                        msg.Attempts++;
                        await db.SaveChangesAsync(stoppingToken);
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

            if (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_options.PollIntervalMs, stoppingToken);
                }
                catch (OperationCanceledException) { }
            }
        }

        _logger.LogInformation("OutboxDispatcher stopped");
    }

    private static IEnumerable<object> ResolveHandlers(IServiceProvider sp, Type clrType)
        => sp.GetServices(typeof(IEventHandler<>).MakeGenericType(clrType))?.Cast<object>() ?? Array.Empty<object>();

    private static Task InvokeHandler(object handler, object envelope, CancellationToken ct)
    {
        var method = handler.GetType().GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance)!;
        return (Task)method.Invoke(handler, new[] { envelope, ct })!;
    }
} 