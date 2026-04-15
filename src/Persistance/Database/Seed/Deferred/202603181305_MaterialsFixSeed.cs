using Microsoft.EntityFrameworkCore;
using rbkApiModules.Commons.Core;
using rbkApiModules.Commons.Relational;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PaintingProjectsManagment.Database;

public class MaterialsFixSeed : IDeferredSeedStep
{
    public string Id => "2026-03-18 13h05m: Fix tenant materials in the projections created during the seed";

    public EnvironmentUsage EnvironmentUsage => EnvironmentUsage.Development | EnvironmentUsage.Staging | EnvironmentUsage.Testing;

    public Type DbContextType => typeof(DatabaseContext);

    public async Task ExecuteAsync(DbContext context, IServiceProvider serviceProvider)
    {
        var timeout = DateTime.UtcNow.AddSeconds(180);

        var unprocessedMessages = int.MaxValue;
        do
        {
            unprocessedMessages = await context.Set<IntegrationOutboxMessage>().CountAsync(x => x.ProcessedUtc == null);

            if (unprocessedMessages > 0)
            {
                await Task.Delay(500);
            }
        } while (unprocessedMessages >0 && DateTime.UtcNow < timeout);

        if (unprocessedMessages > 0)
        {
            Debugger.Break();

            throw new DatabaseSeedException(null, $"Could not process all integration events to proceed with the seed.");
        }

        context.Database.ExecuteSqlRaw("update public.\"projects.projections.materials\" set \"Tenant\" = 'RODRIGO.BASNIAK'");
    } 
}
