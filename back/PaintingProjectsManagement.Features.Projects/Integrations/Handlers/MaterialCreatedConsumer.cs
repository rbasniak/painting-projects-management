using PaintingProjectsManagement.Features.Materials.Abstractions;

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

        var entity = await _context.Set<ReadOnlyMaterial>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Tenant == envelope.TenantId && x.Id == @event.MaterialId, cancellationToken);

        if (entity == null)
        {
            // TODO: change PricePerUnit to ValueObject called UnitPrice that contains unit, price and currency (money type). UnitPrice should be an owned entity in the ReadOnlyMaterial entity
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
        else
        {
            entity = entity with
            {
                Name = @event.Name,
                Unit = @event.PackageContentUnit,
                UpdatedUtc = DateTime.UtcNow,
                PricePerUnit = @event.PackageContentAmount == 0 ? 0 : @event.PackagePriceAmount / @event.PackageContentAmount
            };

            _context.Update(entity);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
