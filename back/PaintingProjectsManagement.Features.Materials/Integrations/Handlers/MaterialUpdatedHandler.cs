using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaintingProjectsManagement.Features.Materials.Abstractions;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials;

public sealed class MaterialUpdatedHandler :
    IEventHandler<MaterialPackageContentChanged>,
    IEventHandler<MaterialPackagePriceChanged>,
    IEventHandler<MaterialNameChanged>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IIntegrationOutbox _outbox;
    private readonly IIntegrationDeliveryScheduler _scheduler;
    private readonly IOptions<OutboxOptions> _outboxOptions;

    public MaterialUpdatedHandler(IServiceScopeFactory scopeFactory, IIntegrationOutbox outbox, IIntegrationDeliveryScheduler scheduler, IOptions<OutboxOptions> outboxOptions)
    {
        _scopeFactory = scopeFactory;
        _outbox = outbox;
        _scheduler = scheduler;
        _outboxOptions = outboxOptions;
    }

    public Task Handle(EventEnvelope<MaterialPackageContentChanged> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    public Task Handle(EventEnvelope<MaterialPackagePriceChanged> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    public Task Handle(EventEnvelope<MaterialNameChanged> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    private async Task PublishUpdated<TEvent>(EventEnvelope<TEvent> envelope, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = _outboxOptions.Value.ResolveDbContext!(scope.ServiceProvider);

        var materialId = envelope.Event switch
        {
            MaterialPackageContentChanged e => e.MaterialId,
            MaterialPackagePriceChanged e => e.MaterialId,
            MaterialNameChanged e => e.MaterialId,
            _ => Guid.Empty
        };

        var material = await dbContext.Set<Material>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == materialId, cancellationToken);

        if (material == null)
        {
            return;
        }

        var integration = new MaterialPackageContentChanged(
            material.Id,
            material.Name,
            material.PackageContent.Amount,
            material.PackageContent.Unit.ToString(),
            material.PackagePrice.Amount,
            material.PackagePrice.CurrencyCode
        );

        var integrationEnvelope = EventEnvelopeFactory.Wrap(
            integration,
            envelope.TenantId,
            envelope.Username,
            envelope.CorrelationId,
            envelope.EventId.ToString()
        );

        var eventId = _outbox.Enqueue(integrationEnvelope);
        await _scheduler.SeedDeliveriesAsync(eventId, integrationEnvelope.Name, integrationEnvelope.Version, cancellationToken);
    }
}

