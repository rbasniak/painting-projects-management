namespace PaintingProjectsManagement.Features.Inventory;

public class ListMyPaints : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/inventory/my-paints", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<IReadOnlyCollection<MyPaintDetails>>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("List My Paints")
        .WithTags("Inventory");
    }

    public class Request : AuthenticatedRequest, IQuery { }

    public class Validator : SmartValidator<Request, UserPaint>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization) { }

        protected override void ValidateBusinessRules() { }
    }

    public class Handler(DbContext _context) : IQueryHandler<Request>
    {
        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var username = request.Identity.Tenant ?? string.Empty;

            var userPaints = await _context.Set<UserPaint>()
                .Where(up => up.Username == username)
                .Include(up => up.PaintColor)
                .ThenInclude(x => x.Line)
                .ThenInclude(x => x!.Brand)
                .OrderBy(up => up.PaintColor.Name)
                .ToListAsync(cancellationToken);

            var details = userPaints.Select(up => new MyPaintDetails
            {
                PaintColorId = up.PaintColorId,
                Name = up.PaintColor.Name,
                HexColor = up.PaintColor.HexColor,
                BrandName = up.PaintColor.Line.Brand.Name,
                LineName = up.PaintColor.Line.Name
            }).ToList();

            return QueryResponse.Success((IReadOnlyCollection<MyPaintDetails>)details);
        }
    }
}
