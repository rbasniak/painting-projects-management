namespace PaintingProjectsManagement.Features.Models;

internal class ListModels : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/models", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
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

    public class Handler(DbContext _context) : IQueryHandler<Request, IReadOnlyCollection<ModelDetails>>
    {

        public async Task<QueryResponse<IReadOnlyCollection<ModelDetails>>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var models = await _context.Set<Model>()
                .Include(x => x.Category)
                .OrderBy(x => x.Category.Name)
                .ThenBy(x => x.Name)    
                .ToListAsync(cancellationToken);

            return QueryResponse.Success(models.Select(ModelDetails.FromModel).AsReadOnly());
        }
    }
}