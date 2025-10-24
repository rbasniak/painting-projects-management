using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialDeletedConsumer : IIntegrationEventHandler<MaterialDeletedV1>
{
    private readonly DbContext _context;

    public MaterialDeletedConsumer(DbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(EventEnvelope<MaterialDeletedV1> envelope, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<ReadOnlyMaterial>().FindAsync([envelope.TenantId, envelope.Event.MaterialId], cancellationToken);

        if (entity != null)
        {
            _context.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
