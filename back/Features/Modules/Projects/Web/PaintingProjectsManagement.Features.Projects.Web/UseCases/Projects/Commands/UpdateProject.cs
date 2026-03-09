using PaintingProjectsManagement.Infrastructure.Common;

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
        private readonly ITenantStorageUsageService _storageUsageService;

        public Validator(DbContext context, ILocalizationService localization, ITenantStorageUsageService storageUsageService) : base(context, localization)
        {
            _storageUsageService = storageUsageService;
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Name)
                .MustAsync(async (request, name, cancellationToken) => 
                    !await Context.Set<Project>().AnyAsync(x => x.Name == name && x.TenantId == request.Identity.Tenant && x.Id != request.Id, cancellationToken))
                .WithMessage("A project with this name already exists.");

            RuleFor(x => x.Base64Image)
                .Must(base64 => string.IsNullOrEmpty(base64) || IsValidBase64Image(base64))
                .WithMessage("Invalid base64 image format. Must be a valid base64 encoded image with proper header.");

            RuleFor(x => x.Base64Image)
                .MustAsync(HaveAvailableQuota).WithMessage("Storage quota exceeded.");

            RuleFor(x => x.EndDate)
                .Must(endDate => !endDate.HasValue || endDate.Value <= DateTime.UtcNow)
                .WithMessage("End date cannot be in the future.");

            RuleFor(x => x)
                .MustAsync((request, cancellationToken) =>
                    ArchivedProjectValidation.IsEditableProjectAsync(Context, request.Identity.Tenant, request.Id, cancellationToken))
                .WithMessage(ArchivedProjectValidation.ReadOnlyMessage);
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

        private async Task<bool> HaveAvailableQuota(Request request, string base64Image, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(base64Image) || !IsValidBase64Image(base64Image))
            {
                return true;
            }

            var existingPictureUrl = await Context.Set<Project>()
                .Where(x => x.Id == request.Id && x.TenantId == request.Identity.Tenant)
                .Select(x => x.PictureUrl)
                .FirstOrDefaultAsync(cancellationToken);

            var bytesToRelease = _storageUsageService.GetFileSizeInBytes(existingPictureUrl);

            return await _storageUsageService.HasQuotaForImageAsync(
                request.Identity.Tenant ?? string.Empty,
                base64Image,
                bytesToRelease,
                cancellationToken);
        }
    }

    public class Handler(DbContext _context, IFileStorage _fileStorage) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .Where(x => x.TenantId == request.Identity.Tenant)
                .Include(x => x.Steps)
                .FirstAsync(x => x.Id == request.Id, cancellationToken);

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
                    folderPath: Path.Combine(request.Identity.Tenant ?? string.Empty, "projects"),
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