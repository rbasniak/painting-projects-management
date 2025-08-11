using System;

namespace rbkApiModules.Commons.Core;

public interface IEventTypeRegistry
{
    bool TryResolve(string name, int version, out Type type);
} 