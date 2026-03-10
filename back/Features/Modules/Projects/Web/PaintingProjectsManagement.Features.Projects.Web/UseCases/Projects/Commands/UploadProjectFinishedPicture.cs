using PaintingProjectsManagement.Features.Subscriptions.Integration;
using PaintingProjectsManagement.Infrastructure.Common;
using rbkApiModules.Core.Utilities;

namespace PaintingProjectsManagement.Features.Projects;

public sealed class UploadProjectFinishedPicture : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/projects/finished-picture", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<UrlReference[]>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Upload Project Finished Picture")
        .WithTags("Projects");
    }

    public sealed class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
        public string Base64Image { get; set; } = string.Empty;
    }

    public sealed class Validator : SmartValidator<Request, Project>
    {
        private readonly ITenantStorageUsageService _storageUsageService;
        private readonly IDispatcher _dispatcher;

        public Validator(
            DbContext context,
            ILocalizationService localization,
            ITenantStorageUsageService storageUsageService,
            IDispatcher dispatcher) : base(context, localization)
        {
            _storageUsageService = storageUsageService;
            _dispatcher = dispatcher;
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Base64Image)
                .NotEmpty()
                .Must(HaveValidExtension)
                .WithMessage("Invalid image format.");

            RuleFor(x => x.Base64Image)
                .MustAsync(HaveAvailableQuota)
                .WithMessage("Storage quota exceeded.");

            RuleFor(x => x)
                .MustAsync(HaveAvailableFinishedPicturesLimit)
                .WithMessage("Project finished picture limit reached for current subscription tier.");

            RuleFor(x => x)
                .MustAsync((request, cancellationToken) =>
                    ArchivedProjectValidation.IsEditableProjectAsync(Context, request.Identity.Tenant, request.ProjectId, cancellationToken))
                .WithMessage(ArchivedProjectValidation.ReadOnlyMessage);
        }

        private static bool HaveValidExtension(string base64Image)
        {
            try
            {
                var extension = ImageUtilities.ExtractExtension(base64Image);
                return extension.Equals("jpg", StringComparison.InvariantCultureIgnoreCase)
                    || extension.Equals("png", StringComparison.InvariantCultureIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> HaveAvailableQuota(Request request, string base64Image, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(base64Image) || !HaveValidExtension(base64Image))
            {
                return true;
            }

            return await _storageUsageService.HasQuotaForImageAsync(
                request.Identity.Tenant ?? string.Empty,
                base64Image,
                bytesToRelease: 0,
                cancellationToken);
        }

        private async Task<bool> HaveAvailableFinishedPicturesLimit(Request request, CancellationToken cancellationToken)
        {
            var entitlementResponse = await _dispatcher.SendAsync(
                new GetSubscriptionEntitlementQuery { TenantId = request.Identity.Tenant },
                cancellationToken);
            if (!entitlementResponse.IsValid || entitlementResponse.Data is null)
            {
                return true;
            }

            var maxPictures = entitlementResponse.Data.MaxProjectFinishedPicturesPerProject;

            if (maxPictures == int.MaxValue)
            {
                return true;
            }

            var count = await Context.Set<ProjectPicture>()
                .Where(x => x.ProjectId == request.ProjectId)
                .CountAsync(cancellationToken);

            return count < maxPictures;
        }
    }

    public sealed class Handler(DbContext context, IFileStorage fileStorage) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await context.Set<Project>()
                .Include(x => x.Pictures)
                .FirstAsync(x => x.Id == request.ProjectId, cancellationToken);

            var baseFileName = $"finished_{project.Id:N}_{DateTime.UtcNow.Ticks}";
            var fullFileName = $"{baseFileName}.{ImageUtilities.ExtractExtension(request.Base64Image)}";

            var pictureUrl = await fileStorage.StoreFileFromBase64Async(
                request.Base64Image,
                fullFileName,
                Path.Combine(request.Identity.Tenant ?? string.Empty, "projects", project.Id.ToString(), "finished"),
                4096,
                4096,
                cancellationToken);

            project.AddFinishedPicture(pictureUrl);
            await context.SaveChangesAsync(cancellationToken);

            var pictures = project.Pictures.Select(x => new UrlReference
            {
                Id = x.Id,
                Url = x.Url
            }).ToArray();

            return CommandResponse.Success(pictures);
        }
    }
}
