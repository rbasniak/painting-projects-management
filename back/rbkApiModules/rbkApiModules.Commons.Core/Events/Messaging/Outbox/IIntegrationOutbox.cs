using System;
using System.Threading;
using System.Threading.Tasks;

namespace rbkApiModules.Commons.Core;

public interface IIntegrationOutbox
{
    Task<Guid> Enqueue<T>(EventEnvelope<T> envelope, CancellationToken ct = default);
}
