namespace PaintingProjectsManagement.Features.Inventory;

public class ListPaintBrands : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/paints/brands", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<IReadOnlyCollection<PaintBrandDetails>>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("List Paint Brands")
        .WithTags("Paint Brands");  
    }

    public class Request : IQuery
    {
    }

    public class Validator : SmartValidator<Request, PaintBrand>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            // No business rules needed for listing
        }
    }

    public class Handler(DbContext _context) : IQueryHandler<Request>
    {

        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var brands = await _context.Set<PaintBrand>()
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var result = brands.Select(PaintBrandDetails.FromModel).AsReadOnly();

            return QueryResponse.Success(result);
        }
    }
}