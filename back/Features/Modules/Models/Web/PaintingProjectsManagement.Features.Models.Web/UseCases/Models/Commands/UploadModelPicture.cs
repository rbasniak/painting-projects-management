using rbkApiModules.Core.Utilities;
using PaintingProjectsManagement.Infrastructure.Common;

namespace PaintingProjectsManagement.Features.Models;

public class UploadModelPicture : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/models/picture", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<ModelDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Upload Model Picture")
        .WithTags("Models");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ModelId { get; set; }
        public string Base64Image { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, Model>
    {
        private readonly ITenantStorageUsageService _storageUsageService;

        public Validator(DbContext context, ILocalizationService localization, ITenantStorageUsageService storageUsageService) : base(context, localization)
        {
            _storageUsageService = storageUsageService;
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Base64Image)
                .NotEmpty()
                .Must(HaveValidExtension).WithMessage("Invalid image format.");

            RuleFor(x => x.Base64Image)
                .MustAsync(HaveAvailableQuota).WithMessage("Storage quota exceeded.");
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

        private async Task<bool> HaveAvailableQuota(Request request, string base64Image, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(base64Image) || !HaveValidExtension(request, base64Image))
            {
                return true;
            }

            return await _storageUsageService.HasQuotaForImageAsync(
                request.Identity.Tenant ?? string.Empty,
                base64Image,
                bytesToRelease: 0,
                cancellationToken);
        }
    }

    public class Handler(DbContext _context, IFileStorage _fileStorage) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var model = await _context.Set<Model>()
                .Include(x => x.Category)
                .FirstAsync(x => x.Id == request.ModelId, cancellationToken);

            string baseFileName = $"model_{model.Id:N}_{DateTime.UtcNow.Ticks}";
            string fullFileName = $"{baseFileName}.{ImageUtilities.ExtractExtension(request.Base64Image)}";

            string pictureUrl = await _fileStorage.StoreFileFromBase64Async(
                request.Base64Image,
                fullFileName,
                Path.Combine(request.Identity.Tenant, "models"),
                2048,
                2048,
                cancellationToken);

            model.AddPicture(pictureUrl);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success(ModelDetails.FromModel(model));
        }
    }
}
