using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

internal class GetProjectDetails : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/projects/{id}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);
            
            return ResultsMapper.FromResponse(result);
        })
        .Produces<ProjectDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Get Project Details")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, IQuery
    {
        public Guid Id { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
        }
    }

    public class Handler(DbContext _context, IDispatcher _dispatcher) : IQueryHandler<Request>
    {

        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .Include(x => x.Steps)
                .Include(x => x.References)
                .Include(x => x.Pictures)
                .Include(x => x.Materials)
                .Include(x => x.Groups)
                    .ThenInclude(s => s.Sections)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            var materialIds = project.Materials.Select(m => m.MaterialId).ToArray();
            
            if (materialIds.Any())
            {
                var materialsRequest = new GetMaterialsForProject.Request
                {
                    MaterialIds = materialIds
                };

                var materialsResponse = await _dispatcher.SendAsync(materialsRequest, cancellationToken);
                
                if (!materialsResponse.IsValid)
                {
                    return QueryResponse.Failure(materialsResponse.Error);
                }

                var materialDetails = materialsResponse.Data
                    .Select(MaterialDetails.FromReadOnlyMaterial)
                    .ToArray();

                return QueryResponse.Success(ProjectDetails.FromModel(project, materialDetails));
            }

            return QueryResponse.Success(ProjectDetails.FromModel(project));
        }
    }
}
