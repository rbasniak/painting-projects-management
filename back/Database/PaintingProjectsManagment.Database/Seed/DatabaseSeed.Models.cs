using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Models;
using System.Diagnostics;

namespace PaintingProjectsManagment.Database;

public partial class DatabaseSeed
{

    private void DevelopmentModelsSeed(DatabaseContext context, IServiceProvider provider)
    {
        if (Environment.MachineName == "RB-DESKTOP")
        {
            // On the specific computer - seed from library and generate SQL file
            ModelsSeeder.SeedFromDisk(context);
            context.SaveChanges();
            
            // Generate SQL seed file
            ModelsSeeder.GenerateSqlSeedFile(context);
        }
        else
        {
            // Not on the specific computer - seed from SQL file
            SeedFromSqlFile(context);
        }
    }
    
    private void SeedFromSqlFile(DatabaseContext context)
    {
        var rootPath = new DirectoryInfo(".");

        var relativePath = Path.Combine("PaintingProjectsManagment.Database", "Seed", "models_seed.sql");

        var seedSqlFile = new FileInfo(Path.Combine(rootPath.FullName, relativePath));

        while (!seedSqlFile.Exists)
        {
            rootPath = rootPath.Parent;
            seedSqlFile = new FileInfo(Path.Combine(rootPath.FullName, relativePath));
        }

        var sql = File.ReadAllLines(seedSqlFile.FullName);

        foreach (var item in sql)
        {
            Debug.WriteLine(item);
            context.Database.ExecuteSqlRaw(item);
        }
    }
}
