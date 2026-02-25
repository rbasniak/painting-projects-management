using rbkApiModules.Core.Utilities;

namespace PaintingProjectsManagement.Features.Projects;

public class UploadProjectReferencePicture : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/projects/reference-picture", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<UrlReference[]>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Upload Project Reference Picture")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
        public string Base64Image { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Base64Image)
                .NotEmpty()
                .Must(HaveValidExtension).WithMessage("Invalid image format.");
        }

        private bool HaveValidExtension(Request request, string base64Image)
        {
            try
            {
                var extension = ImageUtilities.ExtractExtension(base64Image);
                return extension.Equals("jpg", StringComparison.InvariantCultureIgnoreCase) || extension.Equals("png", StringComparison.InvariantCultureIgnoreCase);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    public class Handler(DbContext _context, IFileStorage _fileStorage) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var project = await _context.Set<Project>()
                .Include(x => x.References)
                .FirstAsync(x => x.Id == request.ProjectId, cancellationToken);

            string baseFileName = $"reference_{project.Id:N}_{DateTime.UtcNow.Ticks}";
            string fullFileName = $"{baseFileName}.{ImageUtilities.ExtractExtension(request.Base64Image)}";

            string pictureUrl = await _fileStorage.StoreFileFromBase64Async(
                request.Base64Image,
                fullFileName,
                Path.Combine(request.Identity.Tenant, "projects", project.Id.ToString(), "reference"),
                2048,
                2048,
                cancellationToken);

            project.AddReferencePicture(pictureUrl);
            await _context.SaveChangesAsync(cancellationToken);

            // Return only the updated references list
            var references = project.References.Select(x => new UrlReference
            {
                Id = x.Id,
                Url = x.Url
            }).ToArray();

            return CommandResponse.Success(references);
        }
    }
}
