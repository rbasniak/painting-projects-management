namespace PaintingProjectsManagement.Features.Materials.UseCases.Web;

public class ListMaterials : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/materials", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<IReadOnlyCollection<MaterialDetails>>(StatusCodes.Status200OK)
        .RequireAuthorization()
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
                .Where(x => x.TenantId == request.Identity.Tenant)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var result = materials.Select(MaterialDetails.FromModel).AsReadOnly();
            
            return QueryResponse.Success(result);
        }
    }
}
