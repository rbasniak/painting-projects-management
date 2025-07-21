namespace PaintingProjectsManagement.Features.Models;

public class CreateModel : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/models", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .RequireAuthorization()
        .WithName("Create Model")
        .WithTags("Models");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public string Name { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string Artist { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
        public BaseSize BaseSize { get; set; } = BaseSize.Unknown;
        public FigureSize FigureSize { get; set; } = FigureSize.Unknown;
        public int NumberOfFigures { get; set; } = 1;
        public string Franchise { get; set; } = string.Empty;
        public ModelType ModelType { get; set; } = ModelType.Unknown;
        public int Size { get; set; } = 0; 
    }

    public class Validator : SmartValidator<Request, Model>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Name)
                .MustAsync(async (request, name, cancellationToken) => 
                    !await Context.Set<Model>().AnyAsync(m => m.Name == name && m.Category.TenantId == request.Identity.Tenant, cancellationToken))
                .WithMessage("A model with this name already exists.");

            RuleFor(x => x.Artist)
                .MaximumLength(150);

            RuleFor(x => x.CategoryId)
                .MustAsync(async (request, categoryId, cancellationToken) =>
                    await Context.Set<ModelCategory>().AnyAsync(c => c.Id == categoryId && c.TenantId == request.Identity.Tenant, cancellationToken))
                .WithMessage("The specified category does not exist.");
                
            RuleFor(x => x.NumberOfFigures)
                .GreaterThan(0)
                .WithMessage("Number of figures must be greater than zero.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var category = await _context.Set<ModelCategory>()
                .Where(c => c.TenantId == request.Identity.Tenant)
                .FirstAsync(x => x.Id == request.CategoryId, cancellationToken);
                
            var model = new Model(
                request.Identity.Tenant,
                request.Name,
                category,
                request.Franchise,
                request.ModelType,
                request.Artist,
                request.Tags ?? Array.Empty<string>(),
                request.BaseSize,
                request.FigureSize,
                request.NumberOfFigures,
                request.Size
            );
            
            await _context.AddAsync(model, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var result = ModelDetails.FromModel(model);

            return CommandResponse.Success(result);
        }
    }
}