namespace PaintingProjectsManagement.Features.Paints;

public class CreatePaintBrand : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/paints/brands", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<PaintBrandDetails>(StatusCodes.Status200OK)
        .RequireAuthorization(Claims.MANAGE_PAINTS)
        .WithName("Create Paint Brand")
        .WithTags("Paint Brands");
    }

    public class Request : ICommand
    {
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, PaintBrand>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            // TODO: if we comment this, will SmartValidator still validate the model?
            // Check for unique name constraint (this is handled by database unique index, but we can add a custom message)
            RuleFor(x => x.Name)
                .MustAsync(async (name, cancellationToken) => 
                    !await Context.Set<PaintBrand>().AnyAsync(b => b.Name == name, cancellationToken))
                .WithMessage("A brand with this name already exists.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var brand = new PaintBrand(Guid.NewGuid(), request.Name);
            
            await _context.AddAsync(brand, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var result = PaintBrandDetails.FromModel(brand);

            return CommandResponse.Success(result);
        }
    }
}