namespace PaintingProjectsManagement.Features.Paints;

internal class ListPaintLines : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/paints/lines", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("List Paint Lines")
        .WithTags("Paint Lines");  
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
            var paintLines = await _context.Set<PaintLine>()
                .Include(x => x.Brand)  
                .OrderBy(x => x.Brand.Name)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

            return QueryResponse.Success(paintLines.Select(PaintLineDetails.FromModel).AsReadOnly());
        }
    }
}