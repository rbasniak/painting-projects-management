using Microsoft.EntityFrameworkCore;
using rbkApiModules.Commons.Core;
using rbkApiModules.Commons.Relational;
using rbkApiModules.Identity;

namespace PaintingProjectsManagement.Features.Models;

public class ModelsDbContext : DbContext
{
    public ModelsDbContext(DbContextOptions<ModelsDbContext> options) : base(options)
    {
    }

    public DbSet<Model> Models => Set<Model>();
    public DbSet<ModelCategory> Categories => Set<ModelCategory>();

    public DbSet<OutboxDomainMessage> OutboxDomainMessages => Set<OutboxDomainMessage>();
    public DbSet<OutboxIntegrationEvent> OutboxIntegrationEvents => Set<OutboxIntegrationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModelsDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDomainMessageConfig).Assembly);

        SchemaRegistry.AddRelationalMapping<Model>("models", "models");
        SchemaRegistry.AddRelationalMapping<ModelCategory>("models", "categories");

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
