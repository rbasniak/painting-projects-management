using Microsoft.EntityFrameworkCore;
using rbkApiModules.Commons.Core;
using rbkApiModules.Commons.Relational;
using rbkApiModules.Identity;

namespace PaintingProjectsManagement.Features.Materials;

public class MaterialsDbContext : DbContext
{
    public MaterialsDbContext(DbContextOptions<MaterialsDbContext> options) : base(options)
    {
    }

    public DbSet<Material> Materials => Set<Material>();

    public DbSet<OutboxDomainMessage> OutboxDomainMessages => Set<OutboxDomainMessage>();
    public DbSet<OutboxIntegrationEvent> OutboxIntegrationEvents => Set<OutboxIntegrationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaterialsDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDomainMessageConfig).Assembly);

        SchemaRegistry.AddRelationalMapping<Material>("materials", "materials");

        modelBuilder.Entity<OutboxDomainMessage>().ToTable("OutboxDomainMessages").ExcludeFromMigrations();
        modelBuilder.Entity<OutboxIntegrationEvent>().ToTable("OutboxIntegrationEvents").ExcludeFromMigrations();

        modelBuilder.AddJsonFields();
        modelBuilder.SetupTenants();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeWithoutKindConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableDateTimeWithoutKindConverter>();
    }
}
