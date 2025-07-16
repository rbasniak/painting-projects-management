namespace PaintingProjectsManagement.Features.Models;

internal class CreateModel : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/models", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("Create Model")
        .WithTags("Models");
    }

    public class Request : ICommand<ModelDetails>
    {
        public string Name { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string Artist { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
        public BaseSize BaseSize { get; set; } = BaseSize.Small;
        public FigureSize FigureSize { get; set; } = FigureSize.Normal;
        public int NumberOfFigures { get; set; } = 1;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .MustAsync(async (name, cancellationToken) => 
                    !await context.Set<Model>().AnyAsync(m => m.Name == name, cancellationToken))
                .WithMessage("A model with this name already exists.");

            RuleFor(x => x.Artist)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.CategoryId)
                .NotEmpty()
                .MustAsync(async (categoryId, cancellationToken) =>
                    await context.Set<ModelCategory>().AnyAsync(c => c.Id == categoryId, cancellationToken))
                .WithMessage("The specified category does not exist.");
                
            RuleFor(x => x.NumberOfFigures)
                .GreaterThan(0)
                .WithMessage("Number of figures must be greater than zero.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request, ModelDetails>
    {

        public async Task<CommandResponse<ModelDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var category = await _context.Set<ModelCategory>()
                .FirstAsync(x => x.Id == request.CategoryId, cancellationToken);
                
            var model = new Model(
                request.Name,
                category,
                request.Artist,
                request.Tags ?? Array.Empty<string>(),
                request.BaseSize,
                request.FigureSize,
                request.NumberOfFigures
            );
            
            await _context.AddAsync(model, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success(ModelDetails.FromModel(model));
        }
    }
}