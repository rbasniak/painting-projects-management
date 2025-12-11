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
            var project = await _context.Set<Project>()
                .Include(x => x.Steps)
                .FirstAsync(x => x.Id == request.ProjectId && x.TenantId == request.Identity.Tenant, cancellationToken);

            var stepsByType = project.Steps
                .GroupBy(s => s.Step)
                .ToDictionary(
                    g => new EnumReference(g.Key),
                    g => g.Select(step => new ProjectStepDetails
                    {
                        Id = step.Id,
                        Step = new EnumReference(step.Step),
                        Date = step.Date,
                        DateFormatted = step.Date.ToString("yyyy-MM-dd HH:mm"),
                        DurationInHours = step.Duration,
                        DurationFormatted = FormatDuration(step.Duration)
                    }).OrderBy(s => s.Date).ToList()
                );

            var result = new ProjectStepsGrouped
            {
                StepsByType = stepsByType
            };

            return QueryResponse.Success(result);
        }

        private static string FormatDuration(double hours)
        {
            var totalMinutes = (int)(hours * 60);
            var h = totalMinutes / 60;
            var m = totalMinutes % 60;

            if (h > 0 && m > 0)
            {
                return $"{h}h {m}m";
            }
            else if (h > 0)
            {
                return $"{h}h";
            }
            else
            {
                return $"{m}m";
            }
        }
    }
}