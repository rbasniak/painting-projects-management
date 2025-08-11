using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace rbkApiModules.Commons.Core;

public sealed class ReflectionEventTypeRegistry : IEventTypeRegistry
{
    private readonly Dictionary<(string name, int version), Type> _map;

    public ReflectionEventTypeRegistry(IEnumerable<Assembly> assembliesToScan)
    {
        var assemblies = assembliesToScan?.ToArray() ?? Array.Empty<Assembly>();
        if (assemblies.Length == 0)
        {
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        _map = assemblies
            .SelectMany(a => SafeGetTypes(a))
            .Select(t => (t, attr: t.GetCustomAttribute<EventNameAttribute>()))
            .Where(x => x.attr is not null)
            .GroupBy(x => (x.attr!.Name, x.attr!.Version))
            .ToDictionary(g => g.Key, g => g.Select(x => x.t).First());
    }

    public bool TryResolve(string name, int version, out Type type) => _map.TryGetValue((name, version), out type!);

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch
        {
            return Array.Empty<Type>();
        }
    }
} 