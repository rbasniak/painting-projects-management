using System.Diagnostics.Metrics;

namespace rbkApiModules.Commons.Core;

public static class EventsMeters
{
    public static readonly Meter Meter = new("PaintingProjects.Events", "1.0.0");

    public static readonly Counter<long> IntegrationOutboxMessagesProcessed = Meter.CreateCounter<long>("integration_outbox_messages_processed");
    public static readonly Counter<long> IntegrationOutboxMessagesFailed = Meter.CreateCounter<long>("integration_outbox_messages_failed");
    public static readonly Histogram<double> IntegrationOutboxDispatchDurationMs = Meter.CreateHistogram<double>("integration_outbox_dispatch_duration_ms");

    public static readonly Counter<long> DomainOutboxMessagesProcessed = Meter.CreateCounter<long>("domain_outbox_messages_processed");
    public static readonly Counter<long> DomainOutboxMessagesFailed = Meter.CreateCounter<long>("domain_outbox_messages_failed");
    public static readonly Histogram<double> DomainOutboxDispatchDurationMs = Meter.CreateHistogram<double>("domain_outbox_dispatch_duration_ms");

    public static readonly Counter<long> InboxMessagesProcessed = Meter.CreateCounter<long>("inbox_messages_processed");

    public static readonly Counter<long> DispatcherRequestsProcessed = Meter.CreateCounter<long>("dispatcher_requests_processed");
    public static readonly Counter<long> DispatcherRequestsFailed = Meter.CreateCounter<long>("dispatcher_requests_failed");
    public static readonly Histogram<double> DispatcherRequestDurationMs = Meter.CreateHistogram<double>("dispatcher_request_duration_ms");
}