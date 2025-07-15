namespace PaintingProjectsManagement.Features.Paints;

internal class DeletePaintColor : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/paints/colors/{id}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return TypedResults.Ok();
        })
        .WithName("Delete Paint Color")
        .WithTags("Paint Colors");
    }

    public class Request : ICommand
    {
        public Guid Id { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Id).NotEmpty();
            
            // Check that the paint color exists
            RuleFor(x => x.Id).MustAsync(async (id, cancellationToken) =>
                await context.Set<PaintColor>().AnyAsync(x => x.Id == id, cancellationToken))
                .WithMessage("Paint color with the specified ID does not exist.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var paintColor = await _context.Set<PaintColor>().FirstAsync(x => x.Id == request.Id, cancellationToken);
            
            _context.Remove(paintColor);
            
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}