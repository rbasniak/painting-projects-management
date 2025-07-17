namespace PaintingProjectsManagement.Features.Paints;

internal class ListPaintBrands : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/paints/brands", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("List Paint Brands")
        .WithTags("Paint Brands");  
    }

    public class Request : IQuery
    {
    }

    public class Validator : AbstractValidator<Request>
    {
    }

    public class Handler(DbContext _context) : IQueryHandler<Request>
    {

        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var brands = await _context.Set<PaintBrand>()
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            return QueryResponse.Success(brands.Select(PaintBrandDetails.FromModel).AsReadOnly());
        }
    }
}