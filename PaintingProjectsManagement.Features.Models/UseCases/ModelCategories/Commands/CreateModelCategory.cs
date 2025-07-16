namespace PaintingProjectsManagement.Features.Models;

internal class CreateModelCategory : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/models/categories", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("Create Model Category")
        .WithTags("Model Categories");
    }

    public class Request : ICommand
    {
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .MustAsync(async (name, cancellationToken) => 
                    !await context.Set<ModelCategory>().AnyAsync(c => c.Name == name, cancellationToken))
                .WithMessage("A model category with this name already exists.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var category = new ModelCategory(Guid.NewGuid(), request.Name);
            
            await _context.AddAsync(category, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}