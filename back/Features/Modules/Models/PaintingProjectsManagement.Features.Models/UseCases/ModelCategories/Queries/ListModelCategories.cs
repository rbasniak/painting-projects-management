namespace PaintingProjectsManagement.Features.Models;

public class ListModelCategories : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/models/categories", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<IReadOnlyCollection<ModelCategoryDetails>>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("List Model Categories")
        .WithTags("Model Categories");  
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
            var categories = await _context.Set<ModelCategory>()
                .Where(x => x.TenantId == request.Identity.Tenant)
                .ToListAsync(cancellationToken);

            var result = categories.Select(ModelCategoryDetails.FromModel).AsReadOnly();
            
            return QueryResponse.Success(result);
        }
    }
}