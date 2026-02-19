using PaintingProjectsManagement.Features.Inventory.Web;
using PaintingProjectsManagement.Infrastructure.Common;

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
        .Produces<CatalogDetails>(StatusCodes.Status200OK)
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
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var lines = await _context.Set<PaintLine>()
                .Include(x => x.Brand)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var paints = await _context.Set<PaintColor>()
                .Include(x => x.Line)
                .ThenInclude(x => x!.Brand)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var brandDetails = brands.Select(brand =>
            {
                var brandLines = lines.Where(x => x.BrandId == brand.Id).ToList();
                var lineDetails = brandLines.Select(line =>
                {
                    var linePaints = paints.Where(x => x.LineId == line.Id).ToList();
                    return new PaintLineDetails
                    {
                        Id = line.Id,
                        Name = line.Name,
                        Brand = new EntityReference(line.BrandId, line.Brand.Name),
                        Paints = linePaints.Select(PaintColorDetails.FromModel).ToList()
                    };
                }).ToList();

                return new PaintBrandDetails
                {
                    Id = brand.Id,
                    Name = brand.Name,
                    Lines = lineDetails
                };
            }).ToList();

            var response = new CatalogDetails { Brands = brandDetails };
            return QueryResponse.Success(response);
        }
    } 
}
