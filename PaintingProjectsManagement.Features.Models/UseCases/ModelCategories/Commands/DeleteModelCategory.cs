namespace PaintingProjectsManagement.Features.Models;

internal class DeleteModelCategory : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/models/categories/{id}", async (Guid id, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return Results.Ok();
        })
        .WithName("Delete Model Category")
        .WithTags("Model Categories");
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
                    !await context.Set<Model>().AnyAsync(m => m.CategoryId == id, cancellationToken))
                .WithMessage("Cannot delete a category that has models associated with it.");
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
            var category = await _context.Set<ModelCategory>().FirstAsync(x => x.Id == request.Id, cancellationToken);

            _context.Remove(category);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}