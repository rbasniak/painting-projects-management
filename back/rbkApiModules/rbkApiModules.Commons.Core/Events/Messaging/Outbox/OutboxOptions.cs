using System;
using Microsoft.EntityFrameworkCore;

namespace rbkApiModules.Commons.Core;

public sealed class OutboxOptions
{
    public int BatchSize { get; set; } = 50;
    public int PollIntervalMs { get; set; } = 1000;
    public int MaxAttempts { get; set; } = 5;

    // Required: provide a way to resolve the application's DbContext
    public Func<IServiceProvider, MessagingDbContext>? ResolveSilentDbContext { get; set; }
    public Func<IServiceProvider, MessagingDbContext>? ResolveDbContext { get; set; }
} 