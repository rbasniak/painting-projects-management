using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace rbkApiModules.Commons.Core;

public interface IBrokerSubscriber
{
    Task SubscribeAsync(string queue, IEnumerable<string> topics, Func<string, ReadOnlyMemory<byte>, IReadOnlyDictionary<string, object?>, CancellationToken, Task> handler, CancellationToken cancellationToken);
}
