using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Materials;

internal sealed class MaterialUpdatedHandler :
    IDomainEventHandler<MaterialPackageContentChanged>, 
    IDomainEventHandler<MaterialPackagePriceChanged>, 
    IDomainEventHandler<MaterialNameChanged>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<DomainEventDispatcherOptions> _outboxOptions;
    private readonly IIntegrationOutbox _outbox;

    public MaterialUpdatedHandler(IServiceScopeFactory scopeFactory, IOptions<DomainEventDispatcherOptions> outboxOptions, IIntegrationOutbox outbox)
    {
        _scopeFactory = scopeFactory;
        _outboxOptions = outboxOptions;
        _outbox = outbox;
    }

    public Task HandleAsync(EventEnvelope<MaterialPackageContentChanged> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    public Task HandleAsync(EventEnvelope<MaterialPackagePriceChanged> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    public Task HandleAsync(EventEnvelope<MaterialNameChanged> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    private async Task PublishUpdated<TEvent>(EventEnvelope<TEvent> envelope, CancellationToken cancellationToken)
    {
        var domainEnvelope = envelope;

        using var scope = _scopeFactory.CreateScope();

        var applicationDb = scope.ServiceProvider.GetRequiredService<DbContext>();

        var materialId = domainEnvelope.Event switch
        {
            MaterialPackageContentChanged e => e.MaterialId,
            MaterialPackagePriceChanged e => e.MaterialId,
            MaterialNameChanged e => e.MaterialId,
            _ => Guid.Empty
        };

        var material = await applicationDb.Set<Material>()
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

