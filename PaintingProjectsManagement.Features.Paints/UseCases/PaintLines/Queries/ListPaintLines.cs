namespace PaintingProjectsManagement.Features.Paints;

internal class ListPaintLines : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/paints/lines", async (Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.QueryAsync(new Request(), cancellationToken);

            return TypedResults.Ok(result);
        })
        .WithName("List Paint Lines")
        .WithTags("Paint Lines");  
    }

    public class Request : IQuery<IReadOnlyCollection<PaintLineDetails>>
    {
    }

    public class Validator : AbstractValidator<Request>
    {
    }

    public class Handler : IQueryHandler<Request, IReadOnlyCollection<PaintLineDetails>>
    {
        private readonly DbContext _context;

        public Handler(DbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<PaintLineDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var paintLines = await _context.Set<PaintLine>()
                .Include(x => x.Brand)  
                .OrderBy(x => x.Brand.Name)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

            return paintLines.Select(PaintLineDetails.FromModel).AsReadOnly();
        }
    }
}