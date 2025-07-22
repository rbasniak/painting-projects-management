namespace PaintingProjectsManagement.Features.Models;

public class ListPriorityModels : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/models/prioritized", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<IReadOnlyCollection<ModelDetails>>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("List Priority Models")
        .WithTags("Models");
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
            var models = await _context.Set<Model>()
                .Include(x => x.Category)
                .Where(x => x.TenantId == request.Identity.Tenant)
                .Where(x => x.Score == 5)
                .OrderByDescending(x => x.Priority) // First prioritized models (positive priority)
                .ThenBy(x => x.Category.Name)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var result = models.Select(ModelDetails.FromModel).AsReadOnly();

            return QueryResponse.Success(result);
        }
    }
}