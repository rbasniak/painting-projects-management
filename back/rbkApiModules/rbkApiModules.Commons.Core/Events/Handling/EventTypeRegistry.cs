using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace rbkApiModules.Commons.Core;

public static class EventTypeRegistry
{
    private static readonly ConcurrentDictionary<(string name, int version), Type> NameVersionToType = new();
    private static bool _initialized;

    public static void Initialize(params Assembly[] assemblies)
    {
        if (_initialized) return;
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface) continue;
                var attr = type.GetCustomAttribute<EventNameAttribute>();
                if (attr == null) continue;
                NameVersionToType.TryAdd((attr.Name, attr.Version), type);
            }
        }
        _initialized = true;
    }

    public static bool TryGetType(string name, int version, out Type type)
    {
        if (!_initialized) Initialize();
        return NameVersionToType.TryGetValue((name, version), out type!);
    }
} 