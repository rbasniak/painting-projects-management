using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialCreatedConsumer : IIntegrationEventHandler<MaterialCreatedV1>
{
    private readonly DbContext _context;

    public MaterialCreatedConsumer(DbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(EventEnvelope<MaterialCreatedV1> envelope, CancellationToken cancellationToken)
    {
        var @event = envelope.Event;

        var entity = await _context.Set<ReadOnlyMaterial>().FindAsync([envelope.TenantId, @event.MaterialId], cancellationToken);

        if (entity == null)
        {
            entity = new ReadOnlyMaterial 
            { 
                Tenant = envelope.TenantId, 
                Id = @event.MaterialId,
                Name = @event.Name,
                PricePerUnit = @event.PackageContentAmount == 0 ? 0 : @event.PackagePriceAmount / @event.PackageContentAmount,
                Unit = @event.PackageContentUnit,
                UpdatedUtc = DateTime.UtcNow
            };
            
            _context.Add(entity);
        }

        entity.Name = @event.Name;
        entity.PricePerUnit = pricePerUnit;
        entity.Unit = @event.PackageContentUnit;
        entity.UpdatedUtc = DateTime.UtcNow;

        var count = await _context.SaveChangesAsync(cancellationToken);
    }
}
