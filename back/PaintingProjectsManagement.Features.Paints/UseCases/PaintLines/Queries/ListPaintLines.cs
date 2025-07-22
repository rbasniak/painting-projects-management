namespace PaintingProjectsManagement.Features.Paints;

public class ListPaintLines : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/paints/lines", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<IReadOnlyCollection<PaintLineDetails>>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("List Paint Lines")
        .WithTags("Paint Lines");  
    }

    public class Request : IQuery
    {
    }

    public class Validator : SmartValidator<Request, PaintLine>
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
            var paintLines = await _context.Set<PaintLine>()
                .Include(x => x.Brand)  
                .OrderBy(x => x.Brand.Name)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var result = paintLines.Select(PaintLineDetails.FromModel).AsReadOnly();

            return QueryResponse.Success(result);
        }
    }
}