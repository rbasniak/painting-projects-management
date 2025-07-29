namespace PaintingProjectsManagement.Features.Paints;

public class UpdatePaintBrand : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/paints/brands", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<PaintBrandDetails>(StatusCodes.Status200OK)
        .RequireAuthorization(Claims.MANAGE_PAINTS)
        .WithName("Update Paint Brand")
        .WithTags("Paint Brands");
    }

    public class Request : ICommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, PaintBrand>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            // Check that the new name is not already taken by another brand
            RuleFor(x => x).MustAsync(async (request, cancellationToken) =>
                !await Context.Set<PaintBrand>().AnyAsync(
                    x => x.Name == request.Name && x.Id != request.Id, 
                    cancellationToken))
                .WithMessage("Another brand with this name already exists.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var brand = await _context.Set<PaintBrand>().FirstAsync(x => x.Id == request.Id, cancellationToken);
            brand.UpdateDetails(request.Name);
            await _context.SaveChangesAsync(cancellationToken);
            var result = PaintBrandDetails.FromModel(brand);
            return CommandResponse.Success(result);
        }
    }
}