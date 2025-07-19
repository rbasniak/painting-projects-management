namespace PaintingProjectsManagement.Features.Materials;

public class ListMaterials : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/materials", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("List Materials")
        .WithTags("Materials");  
    }

    public class Request : AuthenticatedRequest, IQuery
    {
    }

    public class Validator : AbstractValidator<Request>
    {

    }

    public class Handler(DbContext _context) : IQueryHandler<Request>
    {

        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var materials = await _context.Set<Material>()
                .Where(m => m.TenantId == request.Identity.Tenant)
                .ToListAsync(cancellationToken);

            return QueryResponse.Success(materials.Select(MaterialDetails.FromModel).AsReadOnly());
        }
    }
}
