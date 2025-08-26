using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaintingProjectsManagement.Features.Models.Abstractions;

namespace PaintingProjectsManagement.Features.Models;

internal sealed class ModelUpdatedHandler :
    IDomainEventHandler<ModelDetailsChanged>,
    IDomainEventHandler<ModelPictureChanged>,
    IDomainEventHandler<ModelRated>,
    IDomainEventHandler<ModelMustHaveChanged>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<DomainEventDispatcherOptions> _outboxOptions;
    private readonly IIntegrationOutbox _outbox;

    public ModelUpdatedHandler(IServiceScopeFactory scopeFactory, IOptions<DomainEventDispatcherOptions> outboxOptions, IIntegrationOutbox outbox)
    {
        _scopeFactory = scopeFactory;
        _outboxOptions = outboxOptions;
        _outbox = outbox;
    }

    public Task HandleAsync(EventEnvelope<ModelDetailsChanged> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    public Task HandleAsync(EventEnvelope<ModelPictureChanged> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    public Task HandleAsync(EventEnvelope<ModelRated> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    public Task HandleAsync(EventEnvelope<ModelMustHaveChanged> envelope, CancellationToken cancellationToken)
        => PublishUpdated(envelope, cancellationToken);

    private async Task PublishUpdated<TEvent>(EventEnvelope<TEvent> envelope, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = _outboxOptions.Value.ResolveDbContext!(scope.ServiceProvider);

        var modelId = envelope.Event switch
        {
            ModelDetailsChanged e => e.ModelId,
            ModelPictureChanged e => e.ModelId,
            ModelRated e => e.ModelId,
            ModelMustHaveChanged e => e.ModelId,
            _ => Guid.Empty
        };

        var model = await dbContext.Set<Model>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == modelId, cancellationToken);

        if (model == null)
        {
            throw new UnexpectedInternalException($"Could not find model id={modelId} when publishing event");
        }

        var integrationEvent = new ModelUpdatedV1(
            model.Id,
            model.Name,
            model.CategoryId,
            model.Artist,
            model.Franchise,
            model.Type.ToString(),
            model.Tags,
            model.Characters,
            model.BaseSize.ToString(),
            model.FigureSize.ToString(),
            model.NumberOfFigures,
            model.SizeInMb,
            model.MustHave,
            model.Score.Value,
            model.PictureUrl
        );

        var integrationEnvelope = EventEnvelopeFactory.Wrap(
            integrationEvent,
            envelope.TenantId,
            envelope.Username,
            envelope.CorrelationId,
            envelope.EventId.ToString()
        );

        await _outbox.Enqueue(integrationEnvelope, cancellationToken);
    }
}
