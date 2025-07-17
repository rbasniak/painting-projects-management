namespace PaintingProjectsManagement.Features.Models;

internal class ListModels : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/models", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("List Models")
        .WithTags("Models");  
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
            var models = await _context.Set<Model>()
                .Include(x => x.Category)
                .OrderBy(x => x.Category.Name)
                .ThenBy(x => x.Name)    
                .ToListAsync(cancellationToken);

            return QueryResponse.Success(models.Select(ModelDetails.FromModel).AsReadOnly());
        }
    }
}