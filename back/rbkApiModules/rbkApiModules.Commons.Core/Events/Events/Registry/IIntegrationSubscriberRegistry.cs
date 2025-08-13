using System.Collections.Generic;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Provides information about integration event subscribers registered in the
/// application.
/// </summary>
public interface IIntegrationSubscriberRegistry
{
    IEnumerable<string> GetSubscribers(string eventName, int version);
}
