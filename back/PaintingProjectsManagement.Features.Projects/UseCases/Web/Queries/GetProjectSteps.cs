using rbkApiModules.Commons.Core.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

public class GetProjectSteps : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/projects/{projectId}/execution/steps", async (Guid projectId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { ProjectId = projectId }, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<ProjectStepsGrouped>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Get Project Steps")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, IQuery
    {
        public Guid ProjectId { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.ProjectId)
                .MustAsync(async (request, id, ct) => await Context.Set<Project>().AnyAsync(p => p.Id == id && p.TenantId == request.Identity.Tenant, ct))
                .WithMessage("Project not found");
        }
    }

    public class Handler(DbContext _context) : IQueryHandler<Request>
    {
        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var steps = await _context.Set<ProjectStepData>()
                .Where(s => s.ProjectId == request.ProjectId)
                .OrderBy(s => s.Date)
                .ToListAsync(cancellationToken);

            var stepsByType = steps
                .GroupBy(s => s.Step)
                .ToDictionary(
                    g => new EnumReference(g.Key),
                    g => g.Select(s => new ProjectStepDetails
                    {
                        Id = s.Id,
                        Step = new EnumReference(s.Step),
                        Date = s.Date,
                        Duration = s.Duration
                    }).ToList()
                );

            var result = new ProjectStepsGrouped
            {
                StepsByType = stepsByType
            };

            return QueryResponse.Success(result);
        }
    }
}
