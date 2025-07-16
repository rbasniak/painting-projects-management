namespace PaintingProjectsManagement.Features.Projects;

internal class ListProjects : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/projects", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("List Projects")
        .WithTags("Projects");  
    }

    public class Request : IQuery<IReadOnlyCollection<ProjectHeader>>
    {
    }

    public class Validator : AbstractValidator<Request>
    {
    }

    public class Handler(DbContext _context) : IQueryHandler<Request, IReadOnlyCollection<ProjectHeader>>
    {

        public async Task<QueryResponse<IReadOnlyCollection<ProjectHeader>>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var projects = await _context.Set<Project>()
                .OrderByDescending(x => x.EndDate == null) // Unfinished projects first
                .ThenByDescending(x => x.EndDate) // Most recently finished next
                .ThenBy(x => x.Name) // Then alphabetically by name
                .ToListAsync(cancellationToken);

            return QueryResponse.Success(projects.Select(ProjectHeader.FromModel).AsReadOnly());
        }
    }
}
