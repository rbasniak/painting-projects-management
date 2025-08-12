using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace rbkApiModules.Commons.Core;

public sealed class EventPublisher : IEventPublisher
{
    private readonly IEventRouter _router;
    private readonly DbContext _dbContext;

    public EventPublisher(IEventRouter router, DbContext dbContext)
    {
        _router = router;
        _dbContext = dbContext;
    }

    public async Task PublishAsync(object envelope, CancellationToken cancellationToken)
    {
        if (envelope == null) throw new ArgumentNullException(nameof(envelope));

        var envelopeType = envelope.GetType();
        if (!envelopeType.IsGenericType || envelopeType.GetGenericTypeDefinition() != typeof(EventEnvelope<>))
        {
            throw new InvalidOperationException("Envelope must be of type EventEnvelope<TEvent>.");
        }

        var eventProperty = envelopeType.GetProperty("Event", BindingFlags.Public | BindingFlags.Instance)!;
        var nameProperty = envelopeType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance)!;
        var versionProperty = envelopeType.GetProperty("Version", BindingFlags.Public | BindingFlags.Instance)!;
        var eventIdProperty = envelopeType.GetProperty("EventId", BindingFlags.Public | BindingFlags.Instance)!;

        var eventPayload = eventProperty.GetValue(envelope)!;
        var eventName = (string)nameProperty.GetValue(envelope)!;
        var eventVersion = (int)versionProperty.GetValue(envelope)!;
        var eventId = (Guid)eventIdProperty.GetValue(envelope)!;

        foreach (var handler in _router.GetHandlers(eventName, eventVersion))
        {
            var handlerName = handler.GetType().FullName!;

            var alreadyProcessed = await _dbContext.Set<InboxMessage>()
                .AnyAsync(x => x.EventId == eventId && x.HandlerName == handlerName, cancellationToken);
            if (alreadyProcessed) continue;

            var handleMethod = handler.GetType().GetMethod("Handle");
            if (handleMethod == null) continue;

            await (Task)handleMethod.Invoke(handler, new[] { envelope, cancellationToken })!;

            _dbContext.Set<InboxMessage>().Add(new InboxMessage
            {
                EventId = eventId,
                HandlerName = handlerName,
                ProcessedUtc = DateTime.UtcNow,
                Attempts = 1
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
} 