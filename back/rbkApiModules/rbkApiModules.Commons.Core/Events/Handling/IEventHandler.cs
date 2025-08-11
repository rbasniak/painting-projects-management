using System.Threading;
using System.Threading.Tasks;

namespace rbkApiModules.Commons.Core;

public interface IEventHandler<TEvent>
{
    Task Handle(EventEnvelope<TEvent> envelope, CancellationToken cancellationToken);
} 