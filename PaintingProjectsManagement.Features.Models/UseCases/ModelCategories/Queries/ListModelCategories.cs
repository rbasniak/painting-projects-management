namespace PaintingProjectsManagement.Features.Models;

internal class ListModelCategories : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/models/categories", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("List Model Categories")
        .WithTags("Model Categories");  
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
            var categories = await _context.Set<ModelCategory>().ToListAsync(cancellationToken);

            return QueryResponse.Success(categories.Select(ModelCategoryDetails.FromModel).AsReadOnly());
        }
    }
}