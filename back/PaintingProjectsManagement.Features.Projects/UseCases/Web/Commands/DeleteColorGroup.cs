namespace PaintingProjectsManagement.Features.Projects;

public class DeleteColorGroup : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/api/projects/color-groups", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Delete Color Group")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
        public Guid ColorGroupId { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.ProjectId)
                .MustAsync(async (request, projectId, cancellationToken) =>
                    await Context.Set<Project>().AnyAsync(
                        p => p.Id == projectId && p.TenantId == request.Identity.Tenant, 
                        cancellationToken))
                .WithMessage("Project not found.");

            RuleFor(x => x.ColorGroupId)
                .MustAsync(async (request, colorGroupId, cancellationToken) =>
                    await Context.Set<ColorGroup>().AnyAsync(
                        g => g.Id == colorGroupId && 
                        g.ProjectId == request.ProjectId &&
                        g.Project.TenantId == request.Identity.Tenant, 
                        cancellationToken))
                .WithMessage("Color group not found or does not belong to the specified project.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .Include(x => x.ColorGroups)
                .FirstAsync(x => x.Id == request.ProjectId && x.TenantId == request.Identity.Tenant, cancellationToken);

            project.RemoveColorGroup(request.ColorGroupId);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}
