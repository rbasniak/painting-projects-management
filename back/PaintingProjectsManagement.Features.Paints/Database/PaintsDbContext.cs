using Microsoft.EntityFrameworkCore;
using rbkApiModules.Commons.Core;
using rbkApiModules.Commons.Relational;
using rbkApiModules.Identity;

namespace PaintingProjectsManagement.Features.Paints;

public class PaintsDbContext : DbContext
{
    public PaintsDbContext(DbContextOptions<PaintsDbContext> options) : base(options)
    {
    }

    public DbSet<PaintBrand> Brands => Set<PaintBrand>();
    public DbSet<PaintLine> Lines => Set<PaintLine>();
    public DbSet<PaintColor> Colors => Set<PaintColor>();

    public DbSet<OutboxDomainMessage> OutboxDomainMessages => Set<OutboxDomainMessage>();
    public DbSet<OutboxIntegrationEvent> OutboxIntegrationEvents => Set<OutboxIntegrationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaintsDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDomainMessageConfig).Assembly);

        SchemaRegistry.AddRelationalMapping<PaintBrand>("paints_catalog", "brands");
        SchemaRegistry.AddRelationalMapping<PaintLine>("paints_catalog", "lines");
        SchemaRegistry.AddRelationalMapping<PaintColor>("paints_catalog", "colors");

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
