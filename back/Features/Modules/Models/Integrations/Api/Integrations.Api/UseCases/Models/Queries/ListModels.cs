namespace PaintingProjectsManagement.Features.Models.Integrations.Api;

public class ListModels : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/integrations/models", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<System.Collections.ObjectModel.ReadOnlyCollection<PaintingProjectsManagement.Features.Models.ModelDetails>>(StatusCodes.Status200OK)
        .RequireAuthorization(ModelsIntegrationsApiAuthentication.PolicyName)
        .WithName("Integrations - List Models")
        .WithTags("Models Integrations");
    }

    public class Request : AuthenticatedRequest, IQuery
    {
    }

    public class Validator : AbstractValidator<Request>
    {
    }

    public class Handler(DbContext context) : IQueryHandler<Request>
    {
        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var models = await context.Set<Model>()
                .Include(x => x.Category)
                .Where(x => x.TenantId == request.Identity.Tenant)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var result = models
                .Select(PaintingProjectsManagement.Features.Models.ModelDetails.FromModel)
                .ToList()
                .AsReadOnly();

            return QueryResponse.Success(result);
        }
    }
}
