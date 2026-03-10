using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

public class GetExecutionMaterialsCatalog : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/projects/execution/materials/available", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<AvailableProjectMaterialDetails[]>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Get Execution Materials Catalog")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, IQuery
    {
    }

    public class Handler(DbContext _context) : IQueryHandler<Request>
    {
        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var materials = await _context.Set<Material>()
                .Where(x => x.Tenant == request.Identity.Tenant)
                .OrderBy(x => x.CategoryId)
                .ThenBy(x => x.Name)
                .Select(x => new AvailableProjectMaterialDetails
                {
                    MaterialId = x.Id,
                    MaterialName = x.Name,
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName,
                    DefaultUnit = x.Unit
                })
                .ToArrayAsync(cancellationToken);

            return QueryResponse.Success(materials);
        }
    }
}
