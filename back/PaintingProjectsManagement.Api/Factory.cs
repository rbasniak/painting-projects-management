using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PaintingProjectsManagment.Database;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Api;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
		try
		{
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

            var connectionString = "Data Source=c:\\temp\\database.db";

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder
                .UseSqlite(connectionString)
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