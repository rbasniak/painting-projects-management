namespace PaintingProjectsManagement.Features.Materials;

public class ListMaterials : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/materials", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return Results.Ok(result);
        })
        .WithName("List Materials")
        .WithTags("Materials");  
    }

    public class Request : IQuery<IReadOnlyCollection<MaterialDetails>>
    {
    }

    public class Validator : AbstractValidator<Request>
    {

    }

    public class Handler(DbContext _context) : IQueryHandler<Request, IReadOnlyCollection<MaterialDetails>>
    {

        public async Task<QueryResponse<IReadOnlyCollection<MaterialDetails>>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var materials = await _context.Set<Material>().ToListAsync(cancellationToken);

            return QueryResponse.Success(materials.Select(MaterialDetails.FromModel).AsReadOnly());
        }
    }
}
