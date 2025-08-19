using Microsoft.EntityFrameworkCore;
using rbkApiModules.Commons.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rbkApiModules.Commons.Core;

public class MessagingDbContext : DbContext
{
    public const string DefaultSchema = "messaging";

    public MessagingDbContext(DbContextOptions<MessagingDbContext> options) : base(options) { }

    public DbSet<OutboxDomainMessage> OutboxDomainMessages => Set<OutboxDomainMessage>();
    public DbSet<OutboxIntegrationEvent> OutboxIntegrationEvents => Set<OutboxIntegrationEvent>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OutboxDomainMessageConfig());
        modelBuilder.ApplyConfiguration(new OutboxIntegrationEventConfig());
        modelBuilder.ApplyConfiguration(new InboxMessageConfig());
    }
}