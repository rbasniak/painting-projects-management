namespace PaintingProjectsManagement.Features.Models;

internal class UpdateModelCategory : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/models/categories", async (Request request, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok(result);
        })
        .WithName("Update Model Category")
        .WithTags("Model Categories");
    }

    public class Request : ICommand<ModelCategoryDetails>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Id)
                .NotEmpty();
                
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .MustAsync(async (request, name, cancellationToken) => 
                    !await context.Set<ModelCategory>().AnyAsync(c => c.Name == name && c.Id != request.Id, cancellationToken))
                .WithMessage("A model category with this name already exists.");
        }
    }

    public class Handler : ICommandHandler<Request, ModelCategoryDetails>
    {
        private readonly DbContext _context;

        public Handler(DbContext context)
        {
            _context = context;
        }

        public async Task<ModelCategoryDetails> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var category = await _context.Set<ModelCategory>().FirstAsync(x => x.Id == request.Id, cancellationToken);

            category.UpdateDetails(request.Name);

            await _context.SaveChangesAsync(cancellationToken);

            return ModelCategoryDetails.FromModel(category);
        }
    }
}