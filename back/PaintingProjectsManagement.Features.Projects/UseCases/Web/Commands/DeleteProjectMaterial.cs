namespace PaintingProjectsManagement.Features.Projects;

public class DeleteProjectMaterial : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/api/projects/{projectId}/materials/{materialId}", async (Guid projectId, Guid materialId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var request = new Request
            {
                ProjectId = projectId,
                MaterialId = materialId
            };
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Delete Project Material")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
        public Guid MaterialId { get; set; }
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
            RuleFor(x => x.MaterialId)
                .MustAsync(async (request, id, ct) => await Context.Set<MaterialForProject>().AnyAsync(m => m.ProjectId == request.ProjectId && m.MaterialId == id, ct))
                .WithMessage("Material not found in project");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .Include(x => x.Materials)
                .FirstAsync(x => x.Id == request.ProjectId, cancellationToken);

            project.RemoveMaterial(request.MaterialId);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}
