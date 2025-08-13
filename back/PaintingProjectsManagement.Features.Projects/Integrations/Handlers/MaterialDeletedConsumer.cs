using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialDeletedConsumer : IIntegrationEventHandler<MaterialDeletedV1>
{
    private readonly DbContext _db;

    public MaterialDeletedConsumer(DbContext db)
    {
        _db = db;
    }

    public async Task Handle(EventEnvelope<MaterialDeletedV1> envelope, CancellationToken cancellationToken)
    {
        var entity = await _db.Set<MaterialCopy>().FindAsync(new object[] { envelope.Event.MaterialId }, cancellationToken);
        if (entity != null)
        {
            _db.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
