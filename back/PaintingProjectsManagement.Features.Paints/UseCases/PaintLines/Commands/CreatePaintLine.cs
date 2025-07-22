namespace PaintingProjectsManagement.Features.Paints;

public class CreatePaintLine : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/paints/lines", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<PaintLineDetails>(StatusCodes.Status200OK)
        .RequireAuthorization(Claims.MANAGE_PAINTS)
        .WithName("Create Brand Line")
        .WithTags("Paint Lines");
    }

    public class Request : ICommand
    {
        public Guid BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, PaintLine>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            // Check for unique name constraint within the same brand (this is handled by database unique index, but we can add a custom message)
            RuleFor(x => x).MustAsync(async (request, cancellationToken) =>
                !await Context.Set<PaintLine>().AnyAsync(x => x.BrandId == request.BrandId && x.Name == request.Name, cancellationToken))
                .WithMessage("A paint line with this name already exists for this brand.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var brand = await _context.Set<PaintBrand>().FirstAsync(b => b.Id == request.BrandId, cancellationToken);
            var line = new PaintLine(brand, Guid.NewGuid(), request.Name);
            
            await _context.AddAsync(line, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var result = PaintLineDetails.FromModel(line);

            return CommandResponse.Success(result);
        }
    }
}