using System.Diagnostics;

namespace rbkApiModules.Commons.Core;

public static class EventsActivities
{
    public static readonly ActivitySource Source = new(EventsMeters.InstrumentationName, EventsMeters.Version);
}
