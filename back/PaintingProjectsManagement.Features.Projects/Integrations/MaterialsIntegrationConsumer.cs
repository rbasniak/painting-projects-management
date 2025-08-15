using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialsIntegrationConsumer : BackgroundService
{
    private readonly IBrokerSubscriber _subscriber;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventTypeRegistry _eventTypeRegistry;
    private readonly IIntegrationSubscriberRegistry _subscriberRegistry;
    private readonly ILogger<MaterialsIntegrationConsumer> _logger;
    private readonly string _queue;

    public MaterialsIntegrationConsumer(IBrokerSubscriber subscriber, IServiceScopeFactory scopeFactory, IEventTypeRegistry eventTypeRegistry, IIntegrationSubscriberRegistry subscriberRegistry, ILogger<MaterialsIntegrationConsumer> logger)
    {
        _subscriber = subscriber;
        _scopeFactory = scopeFactory;
        _eventTypeRegistry = eventTypeRegistry;
        _subscriberRegistry = subscriberRegistry;
        _logger = logger;
        _queue = typeof(MaterialsIntegrationConsumer).FullName!;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _subscriber.SubscribeAsync(_queue, new[] { "materials.*.v1" }, HandleAsync, stoppingToken);
    }

    private async Task HandleAsync(string topic, byte[] payload, CancellationToken ct)
    {
        var json = Encoding.UTF8.GetString(payload);
        var envelope = JsonEventSerializer.Deserialize<EventEnvelope<object>>(json)!;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbContext>();

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var inserted = await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"InboxMessages\"(\"EventId\", \"HandlerName\", \"ProcessedUtc\", \"Attempts\") VALUES ({envelope.EventId}, {_queue}, {DateTime.UtcNow}, 0) ON CONFLICT DO NOTHING", ct);
        if (inserted == 0)
        {
            await tx.CommitAsync(ct);
            return;
        }

        if (!_eventTypeRegistry.TryResolve(envelope.Name, envelope.Version, out var clrType))
        {
            await tx.RollbackAsync(ct);
            throw new InvalidOperationException($"Unknown event {envelope.Name} v{envelope.Version}");
        }

        var envelopeType = typeof(EventEnvelope<>).MakeGenericType(clrType);
        var typedEnvelope = JsonEventSerializer.Deserialize(json, envelopeType);

        foreach (var handlerName in _subscriberRegistry.GetSubscribers(envelope.Name, envelope.Version))
        {
            var handlerType = Type.GetType(handlerName);
            if (handlerType == null)
            {
                continue;
            }
            var handler = scope.ServiceProvider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod("Handle");
            await (Task)method!.Invoke(handler, new object?[] { typedEnvelope!, ct })!;
        }

        await tx.CommitAsync(ct);
    }
}
