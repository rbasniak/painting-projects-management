namespace PaintingProjectsManagement.Features.Projects;

public class UpdateProjectMaterial : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/projects/{projectId}/materials/{materialId}", async (Guid projectId, Guid materialId, Request body, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            body.ProjectId = projectId;
            body.MaterialId = materialId;
            var result = await dispatcher.SendAsync(body, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Update Project Material")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
        public Guid MaterialId { get; set; }
        public double Quantity { get; set; }
        public MaterialUnit Unit { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to 0");
            RuleFor(x => x.Unit).IsInEnum();
            RuleFor(x => x.ProjectId)
                .MustAsync(async (request, id, ct) => await Context.Set<Project>().AnyAsync(p => p.Id == id && p.TenantId == request.Identity.Tenant, ct))
                .WithMessage("Project not found");
            RuleFor(x => x.MaterialId)
                .MustAsync(async (request, materialId, ct) =>
                {
                    var project = await Context.Set<Project>()
                        .Include(p => p.Materials)
                        .FirstAsync(p => p.Id == request.ProjectId && p.TenantId == request.Identity.Tenant, ct);
                    return project.Materials.Any(m => m.MaterialId == materialId);
                })
                .WithMessage("Material not found in project");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .Include(x => x.Materials)
                .FirstAsync(x => x.Id == request.ProjectId && x.TenantId == request.Identity.Tenant, cancellationToken);

            project.UpdateMaterialQuantity(request.MaterialId, request.Quantity, request.Unit);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}