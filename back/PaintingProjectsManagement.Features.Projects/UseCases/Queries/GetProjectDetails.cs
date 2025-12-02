namespace PaintingProjectsManagement.Features.Projects;

public class GetProjectDetails : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/projects/{projectId}", async (Guid projectId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { Id = projectId }, cancellationToken);
            
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

    internal class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
        }
    }

    public class Handler(DbContext context, IProjectCostCalculator projectCostCalculator) : IQueryHandler<Request>
    {
        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await context.Set<Project>()
                .Include(x => x.Steps)
                .Include(x => x.References)
                .Include(x => x.Pictures)
                .Include(x => x.Materials)
                .Include(x => x.ColorGroups)
                    .ThenInclude(x => x.Sections)
                .FirstAsync(x => x.Id == request.Id, cancellationToken);

            // TODO: Get from proper projection table in the future
            var projectCostBreakdown = await projectCostCalculator.CalculateCostAsync(project.Id, "DKK", cancellationToken);

            if (project.Materials.Any())
            {
                var materialIds = project.Materials.Select(x => x.MaterialId).ToArray();

                //var materialsRequest = new GetMaterialsForProject.Request
                //{
                //    MaterialIds = materialIds
                //};

                //var materialsResponse = await _dispatcher.SendAsync(materialsRequest, cancellationToken);
                
                //if (!materialsResponse.IsValid)
                //{
                //    return QueryResponse.Failure(materialsResponse.Error);
                //}

                //var materialDetails = materialsResponse.Data
                //    .Select(materialData =>
                //    {
                //        var projectMaterial = project.Materials.First(projectMaterial => projectMaterial.MaterialId == materialData.Id);
                //        return MaterialDetails.FromModel(materialData, projectMaterial);
                //    })
                //    .ToArray();
            }

            return QueryResponse.Success(ProjectDetails.FromModel(project, projectCostBreakdown));
        }
    }
}
