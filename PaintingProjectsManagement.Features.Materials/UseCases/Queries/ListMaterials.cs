namespace PaintingProjectsManagement.Features.Materials;

public class ListMaterials : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/materials", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("List Materials")
        .WithTags("Materials");  
    }

    public class Request : IQuery
    {
    }

    public class Validator : AbstractValidator<Request>
    {

    }

    public class Handler(DbContext _context) : IQueryHandler<Request>
    {

        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var materials = await _context.Set<Material>().ToListAsync(cancellationToken);

            return QueryResponse.Success(materials.Select(MaterialDetails.FromModel).AsReadOnly());
        }
    }
}
