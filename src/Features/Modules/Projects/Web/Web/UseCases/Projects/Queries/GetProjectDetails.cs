using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PaintingProjectsManagement.Features.Projects;

public class GetProjectDetails : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/projects/{projectId}", async (Guid projectId, string? currency, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request
            {
                Id = projectId,
                Currency = currency
            }, cancellationToken);
            
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
        public string? Currency { get; set; }
    }

    internal class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Id)
                .MustAsync(async (request, id, cancellationToken) =>
                    await Context.Set<Project>().AnyAsync(
                        x => x.Id == id && x.TenantId == request.Identity.Tenant,
                        cancellationToken))
                .WithMessage("Id references a non-existent record.");
        }
    }

    public class Handler(
        DbContext context,
        IProjectCostCalculator projectCostCalculator,
        ILogger<Handler> logger
    ) : IQueryHandler<Request>
    {
        private const string DefaultCurrency = "DKK";

        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await context.Set<Project>()
                .Include(x => x.Steps)
                .Include(x => x.References)
                .Include(x => x.Pictures)
                .Include(x => x.Materials)
                .Include(x => x.ColorGroups)
                    .ThenInclude(x => x.Sections)
                .FirstAsync(
                    x => x.Id == request.Id && x.TenantId == request.Identity.Tenant,
                    cancellationToken);

            // TODO: Get from proper projection table in the future.
            // Cost failures should not block project details dialogs (references/colors/matching flows).
            ProjectCostBreakdown projectCostBreakdown;
            try
            {
                var selectedCurrency = string.IsNullOrWhiteSpace(request.Currency)
                    ? DefaultCurrency
                    : request.Currency.Trim().ToUpperInvariant();

                projectCostBreakdown = await projectCostCalculator.CalculateCostAsync(project.Id, selectedCurrency, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to calculate project cost for project {ProjectId}. Returning empty cost breakdown.", project.Id);
                projectCostBreakdown = new ProjectCostBreakdown
                {
                    ProjectId = project.Id,
                    Electricity = ElectricityCost.Empty(),
                    Labor = new Dictionary<string, LaborCost>(),
                    Materials = new Dictionary<string, IReadOnlyCollection<MaterialsCost>>()
                };
            }


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
