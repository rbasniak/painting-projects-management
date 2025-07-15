namespace PaintingProjectsManagement.Features.Models;

internal class ListModelCategories : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/models/categories", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return Results.Ok(result);
        })
        .WithName("List Model Categories")
        .WithTags("Model Categories");  
    }

    public class Request : IQuery<IReadOnlyCollection<ModelCategoryDetails>>
    {
    }

    public class Validator : AbstractValidator<Request>
    {
    }

    public class Handler(DbContext _context) : IQueryHandler<Request, IReadOnlyCollection<ModelCategoryDetails>>
    {

        public async Task<QueryResponse<IReadOnlyCollection<ModelCategoryDetails>>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var categories = await _context.Set<ModelCategory>().ToListAsync(cancellationToken);

            return QueryResponse.Success(categories.Select(ModelCategoryDetails.FromModel).AsReadOnly());
        }
    }
}