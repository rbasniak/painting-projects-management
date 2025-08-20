// TODO: DONE, REVIEWED

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

    public IntegrationEventPublisher(
        IServiceScopeFactory scopeFactory, 
        IBrokerPublisher publisher, 
        IEventTypeRegistry eventTypeRegistry, 
        ILogger<IntegrationEventPublisher> logger, 
        IOptions<DomainEventDispatcherOptions> options)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(eventTypeRegistry);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _eventTypeRegistry = eventTypeRegistry;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var messagingDbContext = _options.ResolveDbContext!(scope.ServiceProvider);

                // QUIET "is there anything to do?" check (no logs, no telemetry)
                if (!await HasDueIntegrationAsync(messagingDbContext, cancellationToken))
                {
                    await Task.Delay(_options.PollIntervalMs, cancellationToken);
                    
                    continue;
                }

                var iterationId = Guid.CreateVersion7();
                var outerLogExtraData = new Dictionary<string, object>
                {
                    ["IntegrationOutboxDispatcherInstanceId"] = iterationId.ToString("N"),
                };

                using var outerLogScope = _logger.BeginScope(outerLogExtraData);

                var now = DateTimeOffset.UtcNow;
                var claimTtl = TimeSpan.FromMinutes(5);

                // Fetching candidate messages that are not processed yet, not claimed by another instance, and not due for processing
                var candidateIds = await messagingDbContext.IntegrationOutboxMessage
                    .Where(x => x.ProcessedUtc == null)
                    .Where(x => x.DoNotProcessBeforeUtc == null || x.DoNotProcessBeforeUtc <= now)
                    .Where(x => x.ClaimedUntil == null || x.ClaimedUntil < now)
                    .OrderBy(x => x.CreatedUtc)
                    .Select(x => x.Id)
                    .Take(_options.BatchSize)
                    .ToListAsync(cancellationToken);

                // Claim the messages for this instance
                var claimed = await messagingDbContext.IntegrationOutboxMessage
                    .Where(x => candidateIds.Contains(x.Id))
                    .Where(x => x.ClaimedUntil == null || x.ClaimedUntil < now)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.ClaimedBy, x => iterationId)
                        .SetProperty(x => x.ClaimedUntil, x => now.Add(claimTtl)), cancellationToken);

                if (claimed == 0)
                {
                    // Lost the race to another instance, skip this iteration
                    await Task.Delay(_options.PollIntervalMs, cancellationToken);

                    continue;
                }

                // Fetching the messages that were claimed by this instance
                var batch = await messagingDbContext.IntegrationOutboxMessage
                    .Where(x => x.ClaimedBy == iterationId)
                    .OrderBy(x => x.CreatedUtc)
                    .ToListAsync(cancellationToken);

                foreach (var message in batch)
                {
                    var sw = Stopwatch.StartNew();

                    var hasUpstream = TelemetryUtils.TryBuildUpstreamActivityContext(message, out var upstreamContext);

                    var links = hasUpstream ? [new ActivityLink(upstreamContext)] : (IEnumerable<ActivityLink>?)null;

                    using var publisherActivity =
                        EventsTracing.ActivitySource.StartActivity(
                            "integration-outbox.publish",
                            ActivityKind.Producer,
                            default(ActivityContext), // no parent, otherwise it will grow forever until the next http request, use linked activities instead
                            null,
                            links,
                            default);

                    publisherActivity?.SetTag("messaging.system", "rabbitmq");
                    publisherActivity?.SetTag("messaging.destination_kind", "topic");
                    publisherActivity?.SetTag("messaging.message_id", message.Id);
                    publisherActivity?.SetTag("messaging.event.name", message.Name);
                    publisherActivity?.SetTag("messaging.event.version", message.Version);

                    try
                    {
                        if (!_eventTypeRegistry.TryResolve(message.Name, message.Version, out var integrationEventType))
                        {
                            message.Backoff();

                            await messagingDbContext.SaveChangesAsync(cancellationToken);
                            
                            continue;
                        }

                        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(integrationEventType);

                        JsonEventSerializer.Deserialize(message.Payload, envelopeType);

                        var topic = $"{message.Name}.v{message.Version}";

                        // Inject W3C trace headers
                        var headers = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                        
                        if (publisherActivity is not null)
                        {
                            Propagator.Inject(
                                new PropagationContext(publisherActivity.Context, Baggage.Current),
                                headers,
                                static (headers, key, value) => headers[key] = Encoding.UTF8.GetBytes(value));
                        }

                        await _publisher.PublishAsync(topic, Encoding.UTF8.GetBytes(message.Payload), headers, cancellationToken);

                        message.MarkAsProcessed();

                        await messagingDbContext.SaveChangesAsync(cancellationToken);

                        sw.Stop();

                        EventsMeters.IntegrationOutbox_MessagesProcessed.Add(1);
                        EventsMeters.IntegrationOutbox_DispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();

                        EventsMeters.IntegrationOutbox_MessagesFailed.Add(1);
                        EventsMeters.IntegrationOutbox_DispatchDurationMs.Record(sw.Elapsed.TotalMilliseconds);

                        _logger.LogError(ex, "Failed to publish integration event {EventId}", message.Id);
                        
                        message.Backoff();

                        await messagingDbContext.SaveChangesAsync(cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IntegrationEventPublisher)} loop error");
            }

            await Task.Delay(_options.PollIntervalMs, cancellationToken);
        }
    }

    private static Task<bool> HasDueIntegrationAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        using var _ = SuppressInstrumentationScope.Begin();

        var now = DateTimeOffset.UtcNow;

        return dbContext.Set<IntegrationOutboxMessage>()
            .AsNoTracking()
            .AnyAsync(x => x.ProcessedUtc == null
                && (x.DoNotProcessBeforeUtc == null || x.DoNotProcessBeforeUtc <= now)
                && (x.ClaimedUntil == null || x.ClaimedUntil < now), cancellationToken);
    }
}
