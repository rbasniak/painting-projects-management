using System;

namespace rbkApiModules.Commons.Core;

public class InboxMessage
{
    public Guid EventId { get; set; }
    public string HandlerName { get; set; } = default!;
    public DateTime ProcessedUtc { get; set; }
    public int Attempts { get; set; }
} 