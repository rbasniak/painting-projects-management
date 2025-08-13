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
    private readonly IEventTypeRegistry _eventTypeRegistry;
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly OutboxOptions _options;

    public OutboxDispatcher(
        IServiceScopeFactory scopeFactory,
        IEventTypeRegistry registry,
        ILogger<OutboxDispatcher> logger,
        IOptions<OutboxOptions> options)
    {
        _scopeFactory = scopeFactory;
        _eventTypeRegistry = registry;
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
                Guid[] batch = [];
                // 1) Get a batch (short-lived scope, no tracking)
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = _options.ResolveDbContext!(scope.ServiceProvider);
                    var now = DateTime.UtcNow;

                    batch = await context.Set<OutboxMessage>()
                        .AsNoTracking()
                        .Where(x => x.ProcessedUtc == null
                                 && (x.DoNotProcessBeforeUtc == null || x.DoNotProcessBeforeUtc <= now)
                                 && x.Attempts < _options.MaxAttempts)
                        .OrderBy(x => x.CreatedUtc)
                        .Take(_options.BatchSize)
                        .Select(x => x.Id) // get keys only
                        .ToArrayAsync(cancellationToken);
                }

                // 2) Process each message in its own scope/transaction
                foreach (var messageId in batch)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = _options.ResolveDbContext!(scope.ServiceProvider);

                    // (re)load the message in this context
                    var message = await context.Set<OutboxMessage>().FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

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

                    var sw = Stopwatch.StartNew();
                    using var scopeLog = _logger.BeginScope(new Dictionary<string, object>
                    {
                        ["EventId"] = message.Id,
                        ["CorrelationId"] = message.CorrelationId ?? string.Empty,
                        ["Name"] = message.Name,
                        ["Version"] = message.Version,
                        ["Username"] = message.Username,
                        ["TenantId"] = message.TenantId
                    });

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

                            var processedMessage = await context.Set<InboxMessage>().FindAsync(new object[] { message.Id, handlerName }, cancellationToken);

                            if (processedMessage is not null)
                            {
                                continue;
                            }

                            _logger.LogInformation("Dispatching {Name} v{Version} to {Handler}", message.Name, message.Version, handlerName);

                            await InvokeHandler(handler, envelope, cancellationToken);

                            context.Set<InboxMessage>().Add(new InboxMessage
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
                        
                        EventsMeters.OutboxMessagesProcessed.Add(1);
                        EventsMeters.OutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
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

                        var attempts =  + 1;
                        message.Attempts = attempts;
                        message.DoNotProcessBeforeUtc = DateTime.UtcNow.Add(ComputeBackoff(attempts));

                        await context.SaveChangesAsync(cancellationToken);

                        sw.Stop();

                        EventsMeters.OutboxMessagesFailed.Add(1);
                        EventsMeters.OutboxDispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                        
                        _logger.LogError(ex, "Outbox dispatch failed for {Id}", message.Id);
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

    private static IEnumerable<object> ResolveHandlers(IServiceProvider sp, Type clrType)
        => sp.GetServices(typeof(IEventHandler<>).MakeGenericType(clrType))?.Cast<object>() ?? Array.Empty<object>();

    private static Task InvokeHandler(object handler, object envelope, CancellationToken ct)
    {
        var method = handler.GetType().GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance)!;

        return (Task)method.Invoke(handler, new[] { envelope, ct })!;
    }
} 