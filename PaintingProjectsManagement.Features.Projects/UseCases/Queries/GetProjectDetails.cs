using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

internal class GetProjectDetails : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/projects/{id}", async (Guid id, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.QueryAsync(new Request { Id = id }, cancellationToken);
            
            return Results.Ok(result);
        })
        .WithName("Get Project Details")
        .WithTags("Projects");
    }

    public class Request : IQuery<ProjectDetails?>
    {
        public Guid Id { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }

    public class Handler : IQueryHandler<Request, ProjectDetails?>
    {
        private readonly DbContext _context;
        private readonly Dispatcher _dispatcher;

        public Handler(DbContext context, Dispatcher dispatcher)
        {
            _context = context;
            _dispatcher = dispatcher;
        }

        public async Task<ProjectDetails?> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .Include(x => x.Steps)
                .Include(x => x.References)
                .Include(x => x.Pictures)
                .Include(x => x.Materials)
                .Include(x => x.Sections)
                    .ThenInclude(s => s.ColorGroup)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            var materialIds = project.Materials.Select(m => m.MaterialId).ToArray();
            
            if (materialIds.Any())
            {
                var materialsRequest = new GetMaterialsForProject.Request
                {
                    MaterialIds = materialIds
                };

                var materials = await _dispatcher.SendAsync(materialsRequest, cancellationToken);
                
                var materialDetails = materials
                    .Select(MaterialDetails.FromReadOnlyMaterial)
                    .ToArray();

                return ProjectDetails.FromModel(project, materialDetails);
            }

            return ProjectDetails.FromModel(project);
        }
    }
}
