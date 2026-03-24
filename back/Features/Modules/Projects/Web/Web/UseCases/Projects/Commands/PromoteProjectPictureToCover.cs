namespace PaintingProjectsManagement.Features.Projects;

public class PromoteProjectPictureToCover : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/projects/picture/promote", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Promote Project Picture To Cover")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
        public string PictureUrl { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.PictureUrl)
                .NotEmpty()
                .WithMessage("Picture URL is required.");

            RuleFor(x => x)
                .MustAsync(async (request, cancellation) =>
                {
                    var project = await Context.Set<Project>()
                        .Where(x => x.Id == request.ProjectId && x.TenantId == request.Identity.Tenant)
                        .Include(x => x.References)
                        .Include(x => x.Pictures)
                        .FirstOrDefaultAsync(cancellation);

                    if (project == null)
                    {
                        return false;
                    }

                    return project.References.Any(x => x.Url == request.PictureUrl)
                        || project.Pictures.Any(x => x.Url == request.PictureUrl);
                })
                .WithMessage("The specified picture URL must exist in the project's pictures collection.");

            RuleFor(x => x)
                .MustAsync((request, cancellationToken) =>
                    ArchivedProjectValidation.IsEditableProjectAsync(Context, request.Identity.Tenant, request.ProjectId, cancellationToken))
                .WithMessage(ArchivedProjectValidation.ReadOnlyMessage);
        }
    }

    public class Handler(DbContext context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await context.Set<Project>()
                .Where(x => x.Id == request.ProjectId && x.TenantId == request.Identity.Tenant)
                .FirstAsync(cancellationToken);

            project.UpdateCoverPicture(request.PictureUrl);
            await context.SaveChangesAsync(cancellationToken);
            return CommandResponse.Success();
        }
    }
}
