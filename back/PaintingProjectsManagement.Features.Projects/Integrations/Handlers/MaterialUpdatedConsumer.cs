using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialUpdatedConsumer : IIntegrationEventHandler<MaterialPackageContentChanged>
{
    private readonly DbContext _db;

    public MaterialUpdatedConsumer(DbContext db)
    {
        _db = db;
    }

    public async Task Handle(EventEnvelope<MaterialPackageContentChanged> envelope, CancellationToken cancellationToken)
    {
        var e = envelope.Event;
        var pricePerUnit = e.PackageContentAmount == 0 ? 0 : e.PackagePriceAmount / e.PackageContentAmount;
        var entity = await _db.Set<MaterialCopy>().FindAsync(new object[] { e.MaterialId }, cancellationToken);
        if (entity == null)
        {
            entity = new MaterialCopy { Id = e.MaterialId };
            _db.Add(entity);
        }
        entity.Name = e.Name;
        entity.PricePerUnit = pricePerUnit;
        entity.Unit = e.PackageContentUnit;
        entity.UpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
