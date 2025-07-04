namespace PaintingProjectsManagement.Features.Materials;

public class DeleteMaterial : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/materials/{id}", async (Guid id, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return Results.Ok();
        })
        .WithName("Delete Material")
        .WithTags("Materials");
    }

    public class Request : ICommand
    {
        public Guid Id { get; set; } 
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .MustAsync(async (id, cancellationToken) =>
                    await context.Set<Material>().AnyAsync(m => m.Id == id, cancellationToken))
                .WithMessage("Material with the specified ID does not exist.");
        }
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
            var material = await _context.Set<Material>().FirstAsync(x => x.Id == request.Id, cancellationToken);

            _context.Remove(material);

            await _context.SaveChangesAsync(cancellationToken);
        } 
    }

}
