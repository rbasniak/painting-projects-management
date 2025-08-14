using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Materials;

internal sealed class MaterialUpdatedHandler :
    IEventHandler<MaterialPackageContentChanged>, 
    IEventHandler<MaterialPackagePriceChanged>, 
    IEventHandler<MaterialNameChanged>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<OutboxOptions> _outboxOptions;
    private readonly IIntegrationOutbox _outbox;

    public MaterialUpdatedHandler(IServiceScopeFactory scopeFactory, IOptions<OutboxOptions> outboxOptions, IIntegrationOutbox outbox)
    {
        _scopeFactory = scopeFactory;
        _outboxOptions = outboxOptions;
        _outbox = outbox;
    }

    public Task Handle(EventEnvelope<MaterialPackageContentChanged> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    public Task Handle(EventEnvelope<MaterialPackagePriceChanged> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    public Task Handle(EventEnvelope<MaterialNameChanged> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    private async Task PublishUpdated<TEvent>(EventEnvelope<TEvent> envelope, CancellationToken cancellationToken)
    {
        var domainEnvelope = envelope;

        using var scope = _scopeFactory.CreateScope();

        var dbContext = _outboxOptions.Value.ResolveDbContext!(scope.ServiceProvider);

        var materialId = domainEnvelope.Event switch
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
            throw new UnexpectedInternalException($"Could not find material id={materialId} when publishing event");
        }

        var integrationEvent = new MaterialUpdatedV1(
            material.Id,
            material.Name,
            material.PackageContent.Amount,
            material.PackageContent.Unit.ToString(),
            material.PackagePrice.Amount,
            material.PackagePrice.CurrencyCode
        );

        var integrationEnvelope = EventEnvelopeFactory.Wrap(
            integrationEvent,
            domainEnvelope.TenantId,
            domainEnvelope.Username,
            domainEnvelope.CorrelationId,
            domainEnvelope.EventId.ToString()
        );

        await _outbox.Enqueue(integrationEnvelope, cancellationToken);
    }
}

