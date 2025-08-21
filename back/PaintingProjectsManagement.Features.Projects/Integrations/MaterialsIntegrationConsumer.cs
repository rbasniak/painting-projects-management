using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PaintingProjectsManagement.Features.Projects;

public sealed class MaterialsIntegrationConsumer : BackgroundService
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandleCache = new();

    private readonly IBrokerSubscriber _subscriber;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventTypeRegistry _eventTypeRegistry;
    private readonly IIntegrationSubscriberRegistry _subscriberRegistry;
    private readonly ILogger<MaterialsIntegrationConsumer> _logger;
    private readonly string _queue;

    public MaterialsIntegrationConsumer(
        IBrokerSubscriber subscriber,
        IServiceScopeFactory scopeFactory,
        IEventTypeRegistry eventTypeRegistry,
        IIntegrationSubscriberRegistry subscriberRegistry,
        ILogger<MaterialsIntegrationConsumer> logger)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(eventTypeRegistry);
        ArgumentNullException.ThrowIfNull(subscriberRegistry);
        ArgumentNullException.ThrowIfNull(logger);

        _subscriber = subscriber;
        _scopeFactory = scopeFactory;
        _eventTypeRegistry = eventTypeRegistry;
        _subscriberRegistry = subscriberRegistry;
        _logger = logger;
        _queue = typeof(MaterialsIntegrationConsumer).FullName!;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return _subscriber.SubscribeAsync(
            _queue,
            new[] { "materials.*.v1" },
            HandleAsync,
            cancellationToken);
    }

    private async Task HandleAsync(string topic, ReadOnlyMemory<byte> payload, IReadOnlyDictionary<string, object?> headers, CancellationToken cancellationToken)
    {
        var json = Encoding.UTF8.GetString(payload.Span);
         
        var envelopeHeader = JsonEventSerializer.DeserializeHeader(json);
        if (envelopeHeader is null)
        {
            _logger.LogWarning("Invalid envelope. Topic {Topic}", topic);
            return;
        }

        var parent = Propagator.Extract(default, headers, static (x, key) =>
        {
            if (x.TryGetValue(key, out var v) && v is byte[] b) { return new[] { Encoding.UTF8.GetString(b) }; }
            if (x.TryGetValue(key, out var v2) && v2 is string s) { return new[] { s }; }
            return Array.Empty<string>();
        });

        using var activity = EventsTracing.ActivitySource.StartActivity(
            "integration-inbox.consume",
            ActivityKind.Consumer,
            parent.ActivityContext);

        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination_kind", "queue");
        activity?.SetTag("messaging.destination", _queue);
        activity?.SetTag("messaging.message_id", envelopeHeader.EventId);
        activity?.SetTag("messaging.event.name", envelopeHeader.Name);
        activity?.SetTag("messaging.event.version", envelopeHeader.Version);

        using var scope = _scopeFactory.CreateScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();

        if (!_eventTypeRegistry.TryResolve(envelopeHeader.Name, envelopeHeader.Version, out var clrType))
        {
            throw new InvalidOperationException($"Unknown event {envelopeHeader.Name} v{envelopeHeader.Version}");
        }

        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(clrType);
        var typedEnvelope = JsonEventSerializer.Deserialize(json, envelopeType)
            ?? throw new InvalidOperationException($"Cannot deserialize envelope for {envelopeHeader.Name} v{envelopeHeader.Version}");

        var handlerInterface = typeof(IIntegrationEventHandler<>).MakeGenericType(clrType);
        var handlers = scope.ServiceProvider.GetServices(handlerInterface).Cast<object>();

        foreach (var handler in handlers)
        {
            var handlerName = handler.GetType().FullName!;

            // 1) Fence: insert once
            var inserted = await TryInsertInboxOnceAsync(
                databaseContext,
                envelopeHeader.EventId,
                handlerName,
                DateTimeOffset.UtcNow,
                cancellationToken);

            // 2) If not inserted, check current status
            if (!inserted)
            {
                var alreadyProcessed = await databaseContext.InboxMessages
                    .AsNoTracking()
                    .Where(x => x.EventId == envelopeHeader.EventId && x.HandlerName == handlerName)
                    .Select(x => x.ProcessedUtc != null)
                    .SingleAsync(cancellationToken);

                if (alreadyProcessed)
                {
                    continue; // done previously
                }
            }

            // 3) Increment attempts before running the handler
            await databaseContext.InboxMessages
                .Where(x => x.EventId == envelopeHeader.EventId && x.HandlerName == handlerName)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Attempts, x => x.Attempts + 1), cancellationToken);

            // 4) Invoke handler
            var handleMethod = HandleCache.GetOrAdd(handler.GetType(), static x =>
                x.GetMethod("HandleAsync", BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidOperationException($"Handle method not found on {x.FullName}"));

            await (Task)handleMethod.Invoke(handler, new object?[] { typedEnvelope, cancellationToken })!;

            // 5) Mark processed
            await databaseContext.InboxMessages
                .Where(x => x.EventId == envelopeHeader.EventId && x.HandlerName == handlerName)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.ProcessedUtc, x => DateTime.UtcNow), cancellationToken);
        }
    }

    private async Task<bool> TryInsertInboxOnceAsync(DbContext databaseContext, Guid eventId, string handlerName, DateTimeOffset receivedUtc, CancellationToken cancellationToken)
    {
        var names = DatabaseMetadata.For<InboxMessage>(databaseContext);
        var c = names.Columns;

        var sql = $@"
INSERT INTO {names.Table} ({c[nameof(InboxMessage.EventId)]}, {c[nameof(InboxMessage.HandlerName)]}, {c[nameof(InboxMessage.ReceivedUtc)]}, {c[nameof(InboxMessage.Attempts)]})
VALUES (@p0, @p1, @p2, 0)
ON CONFLICT ({c[nameof(InboxMessage.EventId)]}, {c[nameof(InboxMessage.HandlerName)]}) DO NOTHING";

        var rows = await databaseContext.Database.ExecuteSqlRawAsync(sql, new object[] { eventId, handlerName, receivedUtc }, cancellationToken);
        return rows > 0;
    }
}