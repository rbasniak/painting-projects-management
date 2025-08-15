using Microsoft.EntityFrameworkCore;
using rbkApiModules.Commons.Relational;

namespace rbkApiModules.Commons.Core;

public class MessagingDbContext : DbContext
{
    public MessagingDbContext(DbContextOptions<MessagingDbContext> options) : base(options)
    {
    }

    public DbSet<OutboxDomainMessage> OutboxDomainMessages => Set<OutboxDomainMessage>();
    public DbSet<OutboxIntegrationEvent> OutboxIntegrationEvents => Set<OutboxIntegrationEvent>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDomainMessageConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InboxMessageConfig).Assembly);

        SchemaRegistry.AddRelationalMapping<OutboxDomainMessage>("messaging", "OutboxDomainMessages");
        SchemaRegistry.AddRelationalMapping<OutboxIntegrationEvent>("messaging", "OutboxIntegrationEvents");
        SchemaRegistry.AddRelationalMapping<InboxMessage>("messaging", "InboxMessages");

        modelBuilder.AddJsonFields();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeWithoutKindConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableDateTimeWithoutKindConverter>();
    }
}
