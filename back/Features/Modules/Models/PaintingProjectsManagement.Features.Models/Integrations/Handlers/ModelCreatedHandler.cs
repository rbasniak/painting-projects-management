using PaintingProjectsManagement.Features.Models.Abstractions;
using PaintingProjectsManagement.Infrastructure.Common;
using System.Diagnostics;

namespace PaintingProjectsManagement.Features.Models;

internal sealed class ModelCreatedHandler : IDomainEventHandler<ModelCreated>
{
    private readonly IIntegrationOutbox _outbox;
    private readonly DbContext _context;

    public ModelCreatedHandler(IIntegrationOutbox outbox, DbContext context)
    {
        _outbox = outbox;
        _context = context;
    }

    public async Task HandleAsync(EventEnvelope<ModelCreated> envelope, CancellationToken cancellationToken)
    {
        using var span = EventsTracing.ActivitySource.StartActivity("integration.convert", ActivityKind.Internal);
        span?.SetTag("converter.source", nameof(ModelCreated));
        span?.SetTag("converter.target", nameof(ModelCreatedV1));

        var domainEvent = envelope.Event;

        var model = await _context.Set<Model>()
            .Include(x => x.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == domainEvent.ModelId, cancellationToken);

        var integrationEvent = new ModelCreatedV1(
            model.Id,
            model.Name,
            new EntityReference(model.Category.Id, model.Category.Name),
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
            model.CoverPicture,
            model.Pictures
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
