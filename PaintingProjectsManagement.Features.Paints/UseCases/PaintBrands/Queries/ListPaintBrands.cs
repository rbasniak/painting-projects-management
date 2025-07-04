namespace PaintingProjectsManagement.Features.Paints;

internal class ListPaintBrands : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/paints/brands", async (Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.QueryAsync(new Request(), cancellationToken);

            return TypedResults.Ok(result);
        })
        .WithName("List Paint Brands")
        .WithTags("Paint Brands");  
    }

    public class Request : IQuery<IReadOnlyCollection<PaintBrandDetails>>
    {
    }

    public class Validator : AbstractValidator<Request>
    {
    }

    public class Handler : IQueryHandler<Request, IReadOnlyCollection<PaintBrandDetails>>
    {
        private readonly DbContext _context;

        public Handler(DbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<PaintBrandDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var brands = await _context.Set<PaintBrand>()
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            return brands.Select(PaintBrandDetails.FromModel).AsReadOnly();
        }
    }
}