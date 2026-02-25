namespace PaintingProjectsManagement.Features.Models;

public class UpdateModel : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/models", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<ModelDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Update Model")
        .WithTags("Models");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Artist { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string[] Characters { get; set; } = Array.Empty<string>();
        public string Name { get; set; } = string.Empty;
        public BaseSize BaseSize { get; set; }
        public FigureSize FigureSize { get; set; }
        public int NumberOfFigures { get; set; }
        public string Franchise { get; set; } = string.Empty;
        public ModelType Type { get; set; } = ModelType.Unknown;
        public int SizeInMb { get; set; } = 0;
    }

    public class Validator : SmartValidator<Request, Model>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleForEach(x => x.Tags)
                .NotEmpty()
                .WithMessage("Each tag cannot be empty")
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage("Each tag cannot be whitespace")
                .MaximumLength(25)
                .WithMessage("Each tag cannot exceed 25 characters");


            RuleForEach(x => x.Characters)
                .NotEmpty()
                .WithMessage("Each character cannot be empty")
                .Must(character => !string.IsNullOrWhiteSpace(character))
                .WithMessage("Each character cannot be whitespace")
                .MaximumLength(50)
                .WithMessage("Each character cannot exceed 50 characters");

            RuleFor(x => x.NumberOfFigures)
                .GreaterThan(0)
                .WithMessage("NumberOfFigures must be greater than zero");

            RuleFor(x => x.SizeInMb)
                .GreaterThanOrEqualTo(0)
                .WithMessage("SizeInMb must be greater than or equal to zero");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var query = _context.Set<Model>().AsQueryable();

            // Filter by tenant if authenticated
            if (request.IsAuthenticated && request.Identity.HasTenant)
            {
                query = query.Where(x => x.Category.TenantId == request.Identity.Tenant);
            }

            var model = await query
                .Include(x => x.Category)
                .FirstAsync(x => x.Id == request.Id, cancellationToken);

            var category = await _context.Set<ModelCategory>()
                .Where(x => x.TenantId == request.Identity.Tenant)
                .FirstAsync(x => x.Id == request.CategoryId, cancellationToken);

            model.UpdateDetails(
                request.Name,
                category,
                request.Characters ?? Array.Empty<string>(),
                request.Artist,
                request.Tags ?? Array.Empty<string>(),
                request.BaseSize,
                request.FigureSize,
                request.NumberOfFigures,
                request.Franchise,
                request.Type,
                request.SizeInMb
            );

            await _context.SaveChangesAsync(cancellationToken);

            var result = ModelDetails.FromModel(model);

            return CommandResponse.Success(result);
        }
    }
}
