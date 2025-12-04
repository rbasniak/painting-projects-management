namespace PaintingProjectsManagement.Features.Projects;

public class UpdateProjectStep : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/projects/{projectId}/steps/{stepId}", async (Guid projectId, Guid stepId, Request body, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            body.ProjectId = projectId;
            body.StepId = stepId;
            var result = await dispatcher.SendAsync(body, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Update Project Step")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
        public Guid StepId { get; set; }
        public DateTime? Date { get; set; }
        public double? Duration { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Duration)
                .GreaterThan(0)
                .When(x => x.Duration.HasValue)
                .WithMessage("Duration must be greater than 0");
            RuleFor(x => x.ProjectId)
                .MustAsync(async (request, id, ct) => await Context.Set<Project>().AnyAsync(p => p.Id == id && p.TenantId == request.Identity.Tenant, ct))
                .WithMessage("Project not found");
            RuleFor(x => x.StepId)
                .MustAsync(async (request, id, ct) => await Context.Set<ProjectStepData>().AnyAsync(s => s.ProjectId == request.ProjectId && s.Id == id, ct))
                .WithMessage("Step not found in project");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .Include(x => x.Steps)
                .FirstAsync(x => x.Id == request.ProjectId, cancellationToken);

            project.UpdateStep(request.StepId, request.Date, request.Duration);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}
