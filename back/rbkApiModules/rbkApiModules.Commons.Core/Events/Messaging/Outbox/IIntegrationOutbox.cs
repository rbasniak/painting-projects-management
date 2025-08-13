using System;
using System.Threading;
using System.Threading.Tasks;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Service responsible for storing integration events in the outbox.
/// </summary>
public interface IIntegrationOutbox
{
    Guid Enqueue<T>(EventEnvelope<T> envelope);
}
