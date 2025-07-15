namespace PaintingProjectsManagement.Features.Paints;

internal class DeletePaintLine : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/paints/lines/{id}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return TypedResults.Ok();
        })
        .WithName("Delete Brand Line")
        .WithTags("Paint Lines");
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
            
            RuleFor(x => x.Id).MustAsync(async (id, cancellationToken) => 
                await context.Set<PaintLine>().AnyAsync(x => x.Id == id, cancellationToken))
            .WithMessage("Paint line with the specified ID does not exist.");
            
            RuleFor(x => x.Id).MustAsync(async (id, cancellationToken) => 
                !await context.Set<PaintColor>().AnyAsync(x => x.LineId == id, cancellationToken))
            .WithMessage("Cannot delete a paint line that has associated paint colors. Remove the paint colors first.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var paintLine = await _context.Set<PaintLine>().FirstAsync(x => x.Id == request.Id, cancellationToken);
            
            _context.Remove(paintLine);
            
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}