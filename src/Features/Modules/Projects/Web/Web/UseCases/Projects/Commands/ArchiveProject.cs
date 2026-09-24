namespace PaintingProjectsManagement.Features.Projects;

public class ArchiveProject : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/projects/{id}/archive", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<ProjectHeader>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Archive Project")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x)
                .MustAsync((request, cancellationToken) =>
                    ArchivedProjectValidation.IsEditableProjectAsync(Context, request.Identity.Tenant, request.Id, cancellationToken))
                .WithMessage("Project is already archived.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .FirstAsync(x => x.Id == request.Id && x.TenantId == request.Identity.Tenant, cancellationToken);

            project.Archive();
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success(ProjectHeader.FromModel(project));
        }
    }
}
