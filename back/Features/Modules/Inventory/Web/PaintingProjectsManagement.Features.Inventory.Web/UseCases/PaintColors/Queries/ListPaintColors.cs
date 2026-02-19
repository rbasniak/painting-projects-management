namespace PaintingProjectsManagement.Features.Inventory;

public class ListPaintColors : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/paints/colors", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<IReadOnlyCollection<PaintColorDetails>>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("List Paint Colors")
        .WithTags("Paint Colors");  
    }

    public class Request : IQuery
    {
    }

    public class Validator : SmartValidator<Request, PaintColor>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        } 

        protected override void ValidateBusinessRules()
        {
 
        }
    }

    public class Handler(DbContext _context) : IQueryHandler<Request>
    {

        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var paintColors = await _context.Set<PaintColor>()
                .Include(x => x.Line)
                .ThenInclude(x => x.Brand)
                .OrderBy(x => x.Line.Brand.Name)
                .ThenBy(x => x.Line.Name)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var result = paintColors.Select(PaintColorDetails.FromModel).AsReadOnly();

            return QueryResponse.Success(result);
        }
    }
}