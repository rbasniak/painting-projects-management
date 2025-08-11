using System.Threading;
using System.Threading.Tasks;

namespace rbkApiModules.Commons.Core;

public interface IEventPublisher
{
    Task PublishAsync(object envelope, CancellationToken cancellationToken);
} 