using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Simple reflection based registry for integration event subscribers.
/// It scans loaded assemblies for implementations of
/// <see cref="IIntegrationEventHandler{T}"/> and maps them by event name and version.
/// </summary>
public sealed class IntegrationSubscriberRegistry : IIntegrationSubscriberRegistry
{
    private readonly Dictionary<(string Name, int Version), List<string>> _map = new();

    public IntegrationSubscriberRegistry()
    {
        var handlerTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Select(t => new
            {
                Handler = t,
                Interfaces = t.GetInterfaces()
                               .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
            })
            .Where(x => x.Interfaces.Any());

        foreach (var item in handlerTypes)
        {
            foreach (var @interface in item.Interfaces)
            {
                var eventType = @interface.GetGenericArguments()[0];
                var attr = eventType.GetCustomAttribute<EventNameAttribute>();
                if (attr == null)
                {
                    continue;
                }
                var key = (attr.Name, attr.Version);
                if (!_map.TryGetValue(key, out var list))
                {
                    list = new List<string>();
                    _map[key] = list;
                }
                list.Add(item.Handler.FullName!);
            }
        }
    }

    public IEnumerable<string> GetSubscribers(string eventName, int version)
        => _map.TryGetValue((eventName, version), out var list) ? list : Enumerable.Empty<string>();
}
