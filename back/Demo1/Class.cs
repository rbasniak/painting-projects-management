using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Demo1;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        File.AppendAllText("C:\\temp\\connection.txt", $"\r\n Using design time");
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        File.AppendAllText("C:\\temp\\connection.txt", $"\r\n Created config");

        var connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=none;Integrated Security=True;MultipleActiveResultSets=True"; // config.GetConnectionString("DefaultConnection").Replace("**CONTEXT**", "Application");

        File.AppendAllText("C:\\temp\\connection.txt", $"\r\n Connextion from design time={connectionString}");

        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder
            .UseSqlServer(connectionString)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();

        File.AppendAllText("C:\\temp\\connection.txt", $"\r\n Options builder created");

        return new DatabaseContext(optionsBuilder.Options);
    }
}