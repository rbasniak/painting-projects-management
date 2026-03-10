using PaintingProjectsManagement.Features.Subscriptions.Integration;
using PaintingProjectsManagement.Infrastructure.Common;
using rbkApiModules.Core.Utilities;
using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.Features.Models.Integrations.Api;

public class UploadModelPicture : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/integrations/models/picture", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<PaintingProjectsManagement.Features.Models.ModelDetails>(StatusCodes.Status200OK)
        .RequireAuthorization(ModelsIntegrationsApiAuthentication.PolicyName)
        .WithName("Integrations - Upload Model Picture")
        .WithTags("Models Integrations");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        [JsonPropertyName("id")]
        public string InternalId { get; set; } = string.Empty;
        public string Base64Image { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, Model>
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
            RuleFor(x => x.InternalId)
                .NotEmpty()
                .WithMessage("Id is required.")
                .MaximumLength(512)
                .WithMessage("Id cannot exceed 512 characters.")
                .MustAsync(ModelExistsAsync)
                .WithMessage("Id references a non-existent record.");

            RuleFor(x => x.Base64Image)
                .NotEmpty()
                .Must(HaveValidExtension)
                .WithMessage("Invalid image format.");

            RuleFor(x => x.Base64Image)
                .MustAsync(HaveAvailableQuota)
                .WithMessage("Storage quota exceeded.");

            RuleFor(x => x)
                .MustAsync(HaveAvailableModelPicturesLimit)
                .WithMessage("Model picture limit reached for current subscription tier.");
        }

        private bool HaveValidExtension(Request request, string base64Image)
        {
            try
            {
                var extension = ImageUtilities.ExtractExtension(base64Image);
                return extension.Equals("jpg", StringComparison.InvariantCultureIgnoreCase) ||
                       extension.Equals("png", StringComparison.InvariantCultureIgnoreCase);
            }
            catch
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

        private async Task<bool> HaveAvailableModelPicturesLimit(Request request, CancellationToken cancellationToken)
        {
            var entitlementResponse = await _dispatcher.SendAsync(
                new GetSubscriptionEntitlementQuery { TenantId = request.Identity.Tenant },
                cancellationToken);

            if (!entitlementResponse.IsValid || entitlementResponse.Data is null)
            {
                return true;
            }

            var maxPictures = entitlementResponse.Data.MaxModelPicturesPerModel;
            if (maxPictures == int.MaxValue)
            {
                return true;
            }

            var model = await ResolveModelAsync(request, cancellationToken);
            var currentPictures = model?.Pictures.Length ?? 0;

            return currentPictures < maxPictures;
        }

        private async Task<bool> ModelExistsAsync(Request request, string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            var model = await ResolveModelAsync(request, cancellationToken);
            return model is not null;
        }

        private async Task<Model?> ResolveModelAsync(Request request, CancellationToken cancellationToken)
        {
            var normalizedId = request.InternalId.Trim();
            var query = Context.Set<Model>()
                .AsNoTracking()
                .Where(x => x.TenantId == request.Identity.Tenant);

            Model? model = null;
            if (Guid.TryParse(normalizedId, out var parsedId))
            {
                model = await query.FirstOrDefaultAsync(x => x.Id == parsedId, cancellationToken);
            }

            model ??= await query.FirstOrDefaultAsync(x => x.Identity == normalizedId, cancellationToken);

            return model;
        }
    }

    public class Handler(DbContext context, IFileStorage fileStorage) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var normalizedId = request.InternalId.Trim();

            var query = context.Set<Model>()
                .Include(x => x.Category)
                .Where(x => x.TenantId == request.Identity.Tenant);

            Model? model = null;
            if (Guid.TryParse(normalizedId, out var parsedId))
            {
                model = await query.FirstOrDefaultAsync(x => x.Id == parsedId, cancellationToken);
            }

            model ??= await query.FirstAsync(x => x.Identity == normalizedId, cancellationToken);

            var baseFileName = $"model_{model.Id:N}_{DateTime.UtcNow.Ticks}";
            var fullFileName = $"{baseFileName}.{ImageUtilities.ExtractExtension(request.Base64Image)}";

            var pictureUrl = await fileStorage.StoreFileFromBase64Async(
                request.Base64Image,
                fullFileName,
                Path.Combine(request.Identity.Tenant ?? string.Empty, "models"),
                2048,
                2048,
                cancellationToken);

            model.AddPicture(pictureUrl);
            await context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success(PaintingProjectsManagement.Features.Models.ModelDetails.FromModel(model));
        }
    }
}
