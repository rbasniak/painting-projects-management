namespace PaintingProjectsManagement.Features.Projects;

public class ListProjects : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/projects", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<IReadOnlyCollection<ProjectHeader>>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("List Projects")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, IQuery
    {
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }
    }

    public class Handler(DbContext _context) : IQueryHandler<Request>
    {

        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var projects = await _context.Set<Project>()
                .Where(x => x.TenantId == request.Identity.Tenant)
                .OrderByDescending(x => x.EndDate == null) // Unfinished projects first
                .ThenByDescending(x => x.EndDate) // Most recently finished next
                .ThenBy(x => x.Name) // Then alphabetically by name
                .ToListAsync(cancellationToken);

            return QueryResponse.Success(projects.Select(ProjectHeader.FromModel).AsReadOnly());
        }
    }
} 