namespace PaintingProjectsManagement.Features.Projects;

public class DeleteProjectStep : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/api/projects/{projectId}/steps/{stepId}", async (Guid projectId, Guid stepId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var request = new Request
            {
                ProjectId = projectId,
                StepId = stepId
            };
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Delete Project Step")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
        public Guid StepId { get; set; }
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
                .MustAsync(async (request, stepId, ct) =>
                {
                    var project = await Context.Set<Project>()
                        .Include(x => x.Steps)
                        .FirstAsync(x => x.Id == request.ProjectId && x.TenantId == request.Identity.Tenant, ct);
                    return project.Steps.Any(x => x.Id == stepId);
                })
                .WithMessage("Step not found in project");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .Include(x => x.Steps)
                .FirstAsync(x => x.Id == request.ProjectId && x.TenantId == request.Identity.Tenant, cancellationToken);

            project.RemoveStep(request.StepId);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}