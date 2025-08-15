using rbkApiModules.Commons.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintingProjectsManagement.Features.Materials;

public class MaterialsDatabase : DbContext
{
    public MaterialsDatabase(DbContextOptions<MaterialsDatabase> options)
       : base(options)
    {
    }

    public DbSet<Material> Materials => Set<Material>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaterialsDatabase).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SeedHistory).Assembly);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDomainMessageConfig).Assembly);

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
