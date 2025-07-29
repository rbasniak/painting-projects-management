namespace PaintingProjectsManagement.Features.Projects;

public class UpdateProject : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/projects", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<ProjectHeader>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Update Project")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Base64Image { get; set; } = string.Empty;
        public DateTime? EndDate { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Name)
                .MustAsync(async (request, name, cancellationToken) => 
                    !await Context.Set<Project>().AnyAsync(p => p.Name == name && p.TenantId == request.Identity.Tenant && p.Id != request.Id, cancellationToken))
                .WithMessage("A project with this name already exists.");

            RuleFor(x => x.Base64Image)
                .Must(base64 => string.IsNullOrEmpty(base64) || IsValidBase64Image(base64))
                .WithMessage("Invalid base64 image format. Must be a valid base64 encoded image with proper header.");

            RuleFor(x => x.EndDate)
                .Must(endDate => !endDate.HasValue || endDate.Value <= DateTime.UtcNow)
                .WithMessage("End date cannot be in the future.");
        }

        private bool IsValidBase64Image(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                return true; // Allow empty/null for updates
            }
                
            var hasImagePrefix = base64.StartsWith("data:image/") && base64.Contains(";base64,");
            if (!hasImagePrefix)
            {
                return false;
            }
                
            var base64Content = base64.Split(',')[1];
            
            try
            {
                var bytes = Convert.FromBase64String(base64Content);
                return bytes.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }

    public class Handler(DbContext _context, IFileStorage _fileStorage) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .Include(p => p.Steps)
                .FirstAsync(p => p.Id == request.Id, cancellationToken);

            string pictureUrl = project.PictureUrl;

            if (!string.IsNullOrEmpty(request.Base64Image))
            {
                // Delete the old picture if it exists
                if (!string.IsNullOrEmpty(project.PictureUrl))
                {
                    await _fileStorage.DeleteFileAsync(project.PictureUrl, cancellationToken);
                }

                // Store the new picture
                pictureUrl = await _fileStorage.StoreFileFromBase64Async(
                    request.Base64Image,
                    $"project_{project.Id:N}",
                    folderPath: "projects",
                    cancellationToken: cancellationToken);
            }

            // Update the project details
            project.UpdateDetails(
                request.Name,
                pictureUrl,
                project.StartDate,  // Keep the original start date
                request.EndDate
            );

            await _context.SaveChangesAsync(cancellationToken);

            var result = ProjectHeader.FromModel(project);

            return CommandResponse.Success(result);
        }
    }
}