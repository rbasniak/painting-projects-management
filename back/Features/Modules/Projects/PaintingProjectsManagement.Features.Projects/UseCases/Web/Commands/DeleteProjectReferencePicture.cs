namespace PaintingProjectsManagement.Features.Projects;

public class DeleteProjectReferencePicture : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/projects/reference-picture/delete", async (Request data, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(data, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Delete Project Reference Picture")
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
                        .Include(x => x.References)
                        .FirstOrDefaultAsync(x => x.Id == request.ProjectId, cancellation);
                    return project != null && project.References.Any(r => r.Url == request.PictureUrl);
                })
                .WithMessage("The specified picture URL must exist in the project's reference pictures collection.");
        }
    }

    public class Handler(DbContext _context, IFileStorage _fileStorage) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .FirstAsync(x => x.Id == request.ProjectId, cancellationToken);

            if (!string.IsNullOrEmpty(request.PictureUrl))
            {
                await _fileStorage.DeleteFileAsync(request.PictureUrl, cancellationToken);
            }

            project.RemoveReferencePicture(request.PictureUrl);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}
