using System;
using System.Collections.Generic;

namespace rbkApiModules.Commons.Core;

public interface IEventRouter
{
    IEnumerable<object> GetHandlers(string eventName, int version);
} 