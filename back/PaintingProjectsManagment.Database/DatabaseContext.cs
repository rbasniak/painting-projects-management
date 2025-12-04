using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Materials;
using PaintingProjectsManagement.Features.Models;
using PaintingProjectsManagement.Features.Paints;
using PaintingProjectsManagement.Features.Projects;
using rbkApiModules.Authentication;
using rbkApiModules.Commons.Relational;
using rbkApiModules.Identity;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagment.Database;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SeedHistory).Assembly);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaterialConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaintColorConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModelConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DomainOutboxMessagesConfig).Assembly);

        modelBuilder.Entity<MaterialForProject>()
            .HasOne<PaintingProjectsManagement.Features.Materials.Material>()
            .WithMany()
            .HasForeignKey(x => x.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.AddJsonFields();
        modelBuilder.SetupTenants();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeWithoutKindConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableDateTimeWithoutKindConverter>();
    }
}
