namespace PaintingProjectsManagement.Features.Models;

internal class DeleteModel : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/models/{id}", async (Guid id, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return Results.Ok();
        })
        .WithName("Delete Model")
        .WithTags("Models");
    }

    public class Request : ICommand
    {
        public Guid Id { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
    }

    public class Handler : ICommandHandler<Request>
    {
        private readonly DbContext _context;

        public Handler(DbContext context)
        {
            _context = context;
        }

        public async Task HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var model = await _context.Set<Model>().FirstAsync(x => x.Id == request.Id, cancellationToken);

            _context.Remove(model);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}