using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialUpdatedConsumer : IIntegrationEventHandler<MaterialUpdatedV1>
{
    private readonly DbContext _context;

    public MaterialUpdatedConsumer(DbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(EventEnvelope<MaterialUpdatedV1> envelope, CancellationToken cancellationToken)
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

            entity = entity with
            {
                Name = @event.Name,
                Unit = @event.PackageContentUnit,
                UpdatedUtc = DateTime.UtcNow,
                PricePerUnit = @event.PackageContentAmount == 0 ? 0 : @event.PackagePriceAmount / @event.PackageContentAmount
            };

            _context.Add(entity);
        }
    }
}
