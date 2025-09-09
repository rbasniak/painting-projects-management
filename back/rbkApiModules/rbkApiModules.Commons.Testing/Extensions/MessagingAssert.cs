using Microsoft.EntityFrameworkCore;
using rbkApiModules.Commons.Core;
using rbkApiModules.Commons.Core.Messaging;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rbkApiModules.Testing.Core;

public static class MessagingAssert
{
    public static void ShouldNotHaveCreatedDomainEvents(this DbContext context, DateTime afterDate)
    {
        var domainEvents = context.Set<DomainOutboxMessage>().Where(x => x.CreatedUtc >= afterDate).ToArray();
        var integrationEvents = context.Set<IntegrationOutboxMessage>().Where(x => x.CreatedUtc >= afterDate).ToArray();
        var inboxMessages = context.Set<InboxMessage>().Where(x => x.ReceivedUtc >= afterDate).ToArray();

        domainEvents.Length.ShouldBe(0);
    }

    public static void ShouldHaveCreatedDomainEvents(this DbContext context, DateTime afterDate, Dictionary<Type, int> expectedEvents, out EnvelopeHeader[] events)
    {
        var messages = context.Set<DomainOutboxMessage>().Where(x => x.CreatedUtc >= afterDate).ToArray();

        events = messages.Select(x => JsonEventSerializer.DeserializeHeader(x.Payload)).ToArray();

        var uniqueEventTypes = events.GroupBy(x => x.Name).ToArray();

        foreach (var kvp in expectedEvents)
        {
            var searchedEvents = events.Where(x => x.Name == kvp.Key.GetEventName()).ToArray();

            searchedEvents.Length.ShouldBe(kvp.Value, $"Unexpected number of {kvp.Key.GetEventName()} events");
        }
    }
}