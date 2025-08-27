using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features;
using PaintingProjectsManagement.Features.Materials;
using PaintingProjectsManagement.Features.Models;
using PaintingProjectsManagement.Features.Paints;
using PaintingProjectsManagement.Features.Projects;
using rbkApiModules.Commons.Relational;
using rbkApiModules.Identity.Core;
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
        var basePath = new DirectoryInfo(".");

        var seedDirectory = Path.Combine(basePath.Parent.FullName, "PaintingProjectsManagment.Database", "Seed");
        Directory.CreateDirectory(seedDirectory);

        var sqlFilePath = Path.Combine(seedDirectory, "models_seed.sql");

        if (!File.Exists(sqlFilePath))
        {
            throw new FileNotFoundException(sqlFilePath);
        }
        
        var sql = File.ReadAllLines(sqlFilePath);

        foreach (var item in sql)
        {
            Debug.WriteLine(item);
            context.Database.ExecuteSqlRaw(item);
        }
    }
}
