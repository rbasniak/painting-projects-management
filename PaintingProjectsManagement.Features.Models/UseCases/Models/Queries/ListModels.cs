namespace PaintingProjectsManagement.Features.Models;

internal class ListModels : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/models", async (Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return Results.Ok(result);
        })
        .WithName("List Models")
        .WithTags("Models");  
    }

    public class Request : IQuery<IReadOnlyCollection<ModelDetails>>
    {
    }

    public class Validator : AbstractValidator<Request>
    {
    }

    public class Handler : IQueryHandler<Request, IReadOnlyCollection<ModelDetails>>
    {
        private readonly DbContext _context;

        public Handler(DbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<ModelDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var models = await _context.Set<Model>()
                .Include(x => x.Category)
                .OrderBy(x => x.Category.Name)
                .ThenBy(x => x.Name)    
                .ToListAsync(cancellationToken);

            return models.Select(ModelDetails.FromModel).ToList().AsReadOnly();
        }
    }
}