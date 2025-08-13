using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialCreatedConsumer : IIntegrationEventHandler<MaterialCreatedV1>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<OutboxOptions> _options;

    public MaterialCreatedConsumer(IServiceScopeFactory scopeFactory, IOptions<OutboxOptions> options)
    {
        _scopeFactory = scopeFactory;
        _options = options;
    }

    public async Task Handle(EventEnvelope<MaterialCreatedV1> envelope, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = _options.Value.ResolveDbContext!(scope.ServiceProvider);
        var e = envelope.Event;

        var entity = await db.Set<MaterialLocalCopy>().FindAsync(new object[] { e.MaterialId }, ct);
        if (entity == null)
        {
            entity = new MaterialLocalCopy { Id = e.MaterialId };
            db.Set<MaterialLocalCopy>().Add(entity);
        }

        entity.Name = e.Name;
        entity.Unit = e.PackageContentUnit;
        entity.PricePerUnit = e.PackagePriceAmount / (e.PackageContentAmount == 0 ? 1 : e.PackageContentAmount);
        entity.UpdatedUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
