namespace PaintingProjectsManagement.Features.Projects;

public class AddProjectStep : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/projects/{projectId}/steps", async (Guid projectId, Request body, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            body.ProjectId = projectId;
            var result = await dispatcher.SendAsync(body, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Add Project Step")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
        public ProjectStepDefinition Step { get; set; }
        public DateTime Date { get; set; }
        public double Duration { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Duration).GreaterThan(0).WithMessage("Duration must be greater than 0");
            RuleFor(x => x.Step)
                .Must(step => Enum.IsDefined(typeof(ProjectStepDefinition), step))
                .WithMessage("Invalid step type");
            RuleFor(x => x.ProjectId)
                .MustAsync(async (request, id, ct) => await Context.Set<Project>().AnyAsync(p => p.Id == id && p.TenantId == request.Identity.Tenant, ct))
                .WithMessage("Project not found");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .FirstAsync(x => x.Id == request.ProjectId, cancellationToken);

            project.AddExecutionWindow(request.Step, request.Date, request.Duration);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}
