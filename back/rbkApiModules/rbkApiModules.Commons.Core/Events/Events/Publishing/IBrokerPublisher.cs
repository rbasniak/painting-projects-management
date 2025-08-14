using System.Threading;
using System.Threading.Tasks;

namespace rbkApiModules.Commons.Core;

public interface IBrokerPublisher
{
    Task PublishAsync(string topic, byte[] payload, CancellationToken ct = default);
}
