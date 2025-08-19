using System.Diagnostics.Metrics;

namespace rbkApiModules.Commons.Core;

public static class EventsMeters
{
    public const string InstrumentationName = "PaintingProjects.Events";
    public const string Version = "1.0.0";

    public static readonly Meter Meter = new(InstrumentationName, Version);

    public static readonly Counter<long> OutboxMessagesProcessed = Meter.CreateCounter<long>("outbox_messages_processed");
    public static readonly Counter<long> OutboxMessagesFailed = Meter.CreateCounter<long>("outbox_messages_failed");
    public static readonly Histogram<double> OutboxDispatchDurationMs = Meter.CreateHistogram<double>("outbox_dispatch_duration_ms");
} 