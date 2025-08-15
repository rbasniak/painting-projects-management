using PaintingProjectsManagement.Features;
using PaintingProjectsManagement.Features.Models;
using rbkApiModules.Commons.Relational;

namespace PaintingProjectsManagement.Features.Models;

public partial class DatabaseSeed : DatabaseSeedManager<ModelsDatabase>, IDatabaseSeeder
{
    public DatabaseSeed()
    {
        AddSeed("2025-07-19 16:48: Development models seed", new SeedInfo<ModelsDatabase>(DevelopmentModelsSeed));
    }

    private void DevelopmentModelsSeed(ModelsDatabase context, IServiceProvider provider)
    {
        ModelsSeeder.SeedFromDisk(context);
    }
}
