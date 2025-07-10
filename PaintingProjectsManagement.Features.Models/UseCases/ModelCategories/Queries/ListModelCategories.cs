namespace PaintingProjectsManagement.Features.Models;

internal class ListModelCategories : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/models/categories", async (Dispatcher dispatcher, CancellationToken cancellationToken) =>
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

    public class Handler : IQueryHandler<Request, IReadOnlyCollection<ModelCategoryDetails>>
    {
        private readonly DbContext _context;

        public Handler(DbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<ModelCategoryDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var categories = await _context.Set<ModelCategory>().ToListAsync(cancellationToken);

            return categories.Select(ModelCategoryDetails.FromModel).ToList().AsReadOnly();
        }
    }
}