namespace PaintingProjectsManagement.Features.Paints;

internal class ListPaintColors : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/paints/colors", async (Guid? lineId, Guid? brandId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { LineId = lineId, BrandId = brandId }, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<IReadOnlyCollection<PaintColorDetails>>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("List Paint Colors")
        .WithTags("Paint Colors");  
    }

    public class Request : IQuery
    {
        public Guid? LineId { get; set; }
        public Guid? BrandId { get; set; }
    }

    public class Validator : SmartValidator<Request, PaintColor>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            // If lineId is provided, check that it exists
            When(x => x.LineId.HasValue, () => {
                RuleFor(x => x.LineId).MustAsync(async (lineId, cancellationToken) =>
                    await Context.Set<PaintLine>().AnyAsync(x => x.Id == lineId, cancellationToken))
                    .WithMessage("Paint line with the specified ID does not exist.");
            });
            
            // If brandId is provided, check that it exists
            When(x => x.BrandId.HasValue, () => {
                RuleFor(x => x.BrandId).MustAsync(async (brandId, cancellationToken) =>
                    await Context.Set<PaintBrand>().AnyAsync(x => x.Id == brandId, cancellationToken))
                    .WithMessage("Brand with the specified ID does not exist.");
            });
        }
    }

    public class Handler(DbContext _context) : IQueryHandler<Request>
    {

        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            IQueryable<PaintColor> query = _context.Set<PaintColor>()
                .Include(x => x.Line)
                .ThenInclude(x => x.Brand);
            
            if (request.LineId.HasValue)
            {
                query = query.Where(x => x.LineId == request.LineId.Value);
            }
            
            if (request.BrandId.HasValue)
            {
                query = query.Where(x => x.Line.BrandId == request.BrandId.Value);
            }
            
            var paintColors = await query
                .OrderBy(x => x.Line.Brand.Name)
                .ThenBy(x => x.Line.Name)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var result = paintColors.Select(PaintColorDetails.FromModel).AsReadOnly();

            return QueryResponse.Success(result);
        }
    }
}