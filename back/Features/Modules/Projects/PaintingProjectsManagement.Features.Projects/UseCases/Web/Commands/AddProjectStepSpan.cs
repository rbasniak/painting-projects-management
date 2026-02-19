namespace PaintingProjectsManagement.Features.Projects;

public class AddProjectStepSpan : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/projects/building-step/span", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Add Project Building Step Span")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
        public int StepId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.ProjectId)
                .MustAsync(async (request, id, ct) => await Context.Set<Project>().AnyAsync(x => x.Id == id && x.TenantId == request.Identity.Tenant, ct))
                .WithMessage("Project not found");

            RuleFor(x => x.StepId)
                .Must(id => Enum.IsDefined(typeof(ProjectStepDefinition), id))
                .WithMessage("Invalid step id");

            RuleFor(x => x.StartDate)
                .NotEmpty()
                .LessThan(x => x.EndDate)
                .WithMessage("StartDate must be before EndDate");

            RuleFor(x => x.EndDate)
                .NotEmpty();
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>().FirstAsync(x => x.Id == request.ProjectId, cancellationToken);

            var step = (ProjectStepDefinition)request.StepId;
            project.AddExecutionWindow(step, request.StartDate, request.EndDate);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}
