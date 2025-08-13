using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Simple registry that maps integration events to their subscriber handler
/// names. It scans loaded assemblies for implementations of
/// <see cref="IIntegrationEventHandler{TIntegrationEvent}"/>.
/// </summary>
public interface IIntegrationSubscriberRegistry
{
    IEnumerable<string> GetSubscribers(string eventName, int version);
}

public class IntegrationSubscriberRegistry : IIntegrationSubscriberRegistry
{
    private readonly Dictionary<(string, int), List<string>> _map = new();

    public IntegrationSubscriberRegistry(IServiceProvider serviceProvider)
    {
        var handlerTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Select(t => new
            {
                Handler = t,
                Interface = t.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
            })
            .Where(x => x.Interface != null);

        foreach (var item in handlerTypes)
        {
            var eventType = item.Interface!.GenericTypeArguments[0];
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

    public IEnumerable<string> GetSubscribers(string eventName, int version)
    {
        return _map.TryGetValue((eventName, version), out var list) ? list : Enumerable.Empty<string>();
    }
}
