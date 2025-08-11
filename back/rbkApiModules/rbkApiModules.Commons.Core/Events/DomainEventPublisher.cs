using System;

namespace rbkApiModules.Commons.Core;

public sealed class DomainEventPublisher
{
    public void Raise(AggregateRoot aggregateRoot, IDomainEvent domainEvent)
    {
        if (aggregateRoot == null) throw new ArgumentNullException(nameof(aggregateRoot));
        if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));
        aggregateRoot.AddDomainEvent(domainEvent);
    }
} 