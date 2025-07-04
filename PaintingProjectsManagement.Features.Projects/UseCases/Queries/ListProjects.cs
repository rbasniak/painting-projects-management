namespace PaintingProjectsManagement.Features.Projects;

internal class ListProjects : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/projects", async (Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.QueryAsync(new Request(), cancellationToken);

            return Results.Ok(result);
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

    public class Handler : IQueryHandler<Request, IReadOnlyCollection<ProjectHeader>>
    {
        private readonly DbContext _context;

        public Handler(DbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<ProjectHeader>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var projects = await _context.Set<Project>()
                .OrderByDescending(x => x.EndDate == null) // Unfinished projects first
                .ThenByDescending(x => x.EndDate) // Most recently finished next
                .ThenBy(x => x.Name) // Then alphabetically by name
                .ToListAsync(cancellationToken);

            return projects.Select(ProjectHeader.FromModel).AsReadOnly();
        }
    }
}
