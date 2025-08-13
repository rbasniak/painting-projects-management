using System.Threading;
using System.Threading.Tasks;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Contract for consumers of integration events.
/// </summary>
public interface IIntegrationEventHandler<in TIntegrationEvent>
{
    Task Handle(EventEnvelope<TIntegrationEvent> envelope, CancellationToken ct);
}
