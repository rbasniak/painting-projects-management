using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Inventory;

public class GetCatalog : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/inventory/catalog", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<GetCatalogResponse>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Get Catalog")
        .WithTags("Inventory");
    }

    public class Request : IQuery { }

    public class Validator : SmartValidator<Request, PaintBrand>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization) { }

        protected override void ValidateBusinessRules() { }
    }

    public class Handler(DbContext _context) : IQueryHandler<Request>
    {
        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var brands = await _context.Set<PaintBrand>()
                .OrderBy(b => b.Name)
                .ToListAsync(cancellationToken);

            var lines = await _context.Set<PaintLine>()
                .Include(l => l.Brand)
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken);

            var paints = await _context.Set<PaintColor>()
                .Include(p => p.Line)
                .ThenInclude(l => l!.Brand)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);

            var brandDetails = brands.Select(brand =>
            {
                var brandLines = lines.Where(l => l.BrandId == brand.Id).ToList();
                var lineDetails = brandLines.Select(line =>
                {
                    var linePaints = paints.Where(p => p.LineId == line.Id).ToList();
                    return new PaintLineDetails
                    {
                        Id = line.Id,
                        Name = line.Name,
                        Brand = new EntityReference(line.BrandId, line.Brand.Name),
                        Paints = linePaints.Select(PaintColorDetails.FromModelForCatalog).ToList()
                    };
                }).ToList();

                return new PaintBrandDetails
                {
                    Id = brand.Id,
                    Name = brand.Name,
                    Lines = lineDetails
                };
            }).ToList();

            var response = new GetCatalogResponse { Brands = brandDetails };
            return QueryResponse.Success(response);
        }
    }
}
