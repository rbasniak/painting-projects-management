using Microsoft.EntityFrameworkCore;
using rbkApiModules.Commons.Core;
using rbkApiModules.Commons.Relational;
using rbkApiModules.Identity;
using PaintingProjectsManagement.Features.Materials;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectsDbContext : DbContext
{
    public ProjectsDbContext(DbContextOptions<ProjectsDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<OutboxDomainMessage> OutboxDomainMessages => Set<OutboxDomainMessage>();
    public DbSet<OutboxIntegrationEvent> OutboxIntegrationEvents => Set<OutboxIntegrationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectsDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDomainMessageConfig).Assembly);

        SchemaRegistry.AddRelationalMapping<Project>("projects", "projects");
        SchemaRegistry.AddRelationalMapping<ColorGroup>("projects", "project_color_groups");
        SchemaRegistry.AddRelationalMapping<ColorSection>("project", "project_color_sections");
        SchemaRegistry.AddRelationalMapping<ProjectReference>("projects", "picture_references");
        SchemaRegistry.AddRelationalMapping<ProjectPicture>("projects", "pictures");
        SchemaRegistry.AddRelationalMapping<MaterialForProject>("projects", "project_materials");
        SchemaRegistry.AddRelationalMapping<ReadOnlyMaterial>("projects", "ReadOnlyMaterials");

        modelBuilder.Entity<MaterialForProject>()
            .HasOne<Material>()
            .WithMany()
            .HasForeignKey(x => x.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

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
