using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Models;
using rbkApiModules.Commons.Relational;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Models;

public class ModelsDatabase : DbContext
{
    public ModelsDatabase(DbContextOptions<ModelsDatabase> options)
        : base(options)
    {
    } 

    public DbSet<Model> Models { get; set; } = default!;
    public DbSet<ModelCategory> Categories { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModelsDatabase).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SeedHistory).Assembly);

        modelBuilder.AddJsonFields();

        modelBuilder.Entity<OutboxDomainMessage>()
            .ToTable("OutboxDomainMessages", "messaging", x => x.ExcludeFromMigrations());

        modelBuilder.Entity<OutboxIntegrationMessage>()
          .ToTable("OutboxIntegrationMessages", "messaging", x => x.ExcludeFromMigrations());

        modelBuilder.Entity<InboxMessage>()
          .ToTable("InboxMessages", "messaging", x => x.ExcludeFromMigrations());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeWithoutKindConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableDateTimeWithoutKindConverter>();
    }
}
