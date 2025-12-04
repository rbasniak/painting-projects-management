using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PaintingProjectsManagment.Database;
using rbkApiModules.Commons.Core;
using System.Diagnostics.CodeAnalysis;

namespace PaintingProjectsManagement.Api;

[ExcludeFromCodeCoverage(Justification = "It is not called at runtime, just when creating migrations.")]
public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
		try
		{
            var connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=ppm;Include Error Detail=true;Log Parameters=true";

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder
                .UseNpgsql(connectionString)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();

            var context = new DatabaseContext(optionsBuilder.Options);

            var model = context.Model;
            foreach (var entityType in model.GetEntityTypes())
            {
                Console.WriteLine($"Entity: {entityType.Name}, IsOwned: {entityType.IsOwned()}");
            }

            return context;
        }
		catch (Exception ex)
		{
            File.WriteAllText("migration.log", ex.ToBetterString());
			throw;
		}
    }
}