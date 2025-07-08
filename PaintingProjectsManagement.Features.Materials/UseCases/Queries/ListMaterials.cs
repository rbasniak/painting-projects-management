namespace PaintingProjectsManagement.Features.Materials;

public class ListMaterials : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/materials", async (Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.QueryAsync(new Request(), cancellationToken);

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

    public class Handler : IQueryHandler<Request, IReadOnlyCollection<MaterialDetails>>
    {
        private readonly DbContext _context;

        public Handler(DbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<MaterialDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var materials = await _context.Set<Material>().ToListAsync(cancellationToken);

            return materials.Select(MaterialDetails.FromModel).AsReadOnly();
        }
    }
}
