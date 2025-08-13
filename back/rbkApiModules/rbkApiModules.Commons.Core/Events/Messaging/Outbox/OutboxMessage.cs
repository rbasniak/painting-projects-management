using System;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Legacy alias for <see cref="OutboxDomainMessage"/> to maintain backward compatibility
/// with existing migrations and code. New code should use <see cref="OutboxDomainMessage"/>.
/// </summary>
[Obsolete("Use OutboxDomainMessage instead")] 
public class OutboxMessage : OutboxDomainMessage
{
}
