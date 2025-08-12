using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace rbkApiModules.Commons.Core;

public sealed class DiEventRouter : IEventRouter
{
    private readonly IServiceProvider _serviceProvider;

    public DiEventRouter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<object> GetHandlers(string eventName, int version)
    {
        if (!EventTypeRegistry.TryGetType(eventName, version, out var eventType)) yield break;

        var handlerOpenType = typeof(IEventHandler<>);
        var handlerClosedType = handlerOpenType.MakeGenericType(eventType);
        var handlers = _serviceProvider.GetServices(handlerClosedType);
        foreach (var handler in handlers)
        {
            if (handler != null)
            {
                yield return handler;
            }
        }
    }
} 