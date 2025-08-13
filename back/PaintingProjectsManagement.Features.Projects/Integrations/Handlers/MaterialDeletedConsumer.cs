using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialDeletedConsumer : IIntegrationEventHandler<MaterialDeletedV1>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<OutboxOptions> _options;

    public MaterialDeletedConsumer(IServiceScopeFactory scopeFactory, IOptions<OutboxOptions> options)
    {
        _scopeFactory = scopeFactory;
        _options = options;
    }

    public async Task Handle(EventEnvelope<MaterialDeletedV1> envelope, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = _options.Value.ResolveDbContext!(scope.ServiceProvider);
        var entity = await db.Set<MaterialLocalCopy>().FindAsync(new object[] { envelope.Event.MaterialId }, ct);
        if (entity != null)
        {
            db.Remove(entity);
            await db.SaveChangesAsync(ct);
        }
    }
}
