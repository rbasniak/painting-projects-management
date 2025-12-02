using PaintingProjectsManagement.Features.Materials.Abstractions;

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

        var entity = await _context.Set<ReadOnlyMaterial>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Tenant == envelope.TenantId && x.Id == @event.MaterialId, cancellationToken);

        if (entity == null)
        {
            entity = new ReadOnlyMaterial
            {
                Tenant = envelope.TenantId,
                Id = @event.MaterialId,
                Name = @event.Name,
                CategoryId = @event.CategoryId,
                CategoryName = @event.CategoryName,
                PricePerUnit = new Money(@event.PackageContentAmount == 0 ? 0 : @event.PackagePriceAmount / @event.PackageContentAmount, "USD"),
                Unit = UnitsHelper.Convert(@event.PackageContentUnit),
                UpdatedUtc = DateTime.UtcNow
            };

            _context.Add(entity);
        }
        else
        {
            entity = entity with
            {
                Name = @event.Name,
                Unit = UnitsHelper.Convert(@event.PackageContentUnit),
                UpdatedUtc = DateTime.UtcNow,
                PricePerUnit = new Money(@event.PackageContentAmount == 0 ? 0 : @event.PackagePriceAmount / @event.PackageContentAmount, "USD")
            };

            _context.Update(entity);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
