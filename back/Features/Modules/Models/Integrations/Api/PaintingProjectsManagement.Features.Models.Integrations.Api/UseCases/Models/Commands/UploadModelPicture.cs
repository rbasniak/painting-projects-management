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
        private readonly IModelPictureUploadPolicyService _uploadPolicyService;

        public Validator(
            DbContext context,
            ILocalizationService localization,
            IModelPictureUploadPolicyService uploadPolicyService) : base(context, localization)
        {
            _uploadPolicyService = uploadPolicyService;
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
                .Must((_, base64Image) => _uploadPolicyService.HasValidImageExtension(base64Image))
                .WithMessage("Invalid image format.");

            RuleFor(x => x.Base64Image)
                .MustAsync(HaveAvailableQuota)
                .WithMessage("Storage quota exceeded.");

            RuleFor(x => x)
                .MustAsync(HaveAvailableModelPicturesLimit)
                .WithMessage("Model picture limit reached for current subscription tier.");
        }

        private async Task<bool> HaveAvailableQuota(Request request, string base64Image, CancellationToken cancellationToken)
        {
            return await _uploadPolicyService.HasAvailableQuotaAsync(
                request.Identity.Tenant ?? string.Empty,
                base64Image,
                cancellationToken);
        }

        private async Task<bool> HaveAvailableModelPicturesLimit(Request request, CancellationToken cancellationToken)
        {
            var model = await ResolveModelAsync(request, cancellationToken);
            var currentPictures = model?.Pictures.Length ?? 0;

            return await _uploadPolicyService.HasAvailableModelPicturesLimitAsync(
                request.Identity.Tenant ?? string.Empty,
                currentPictures,
                cancellationToken);
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

    public class Handler(DbContext context, IModelPictureStorageService pictureStorageService) : ICommandHandler<Request>
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

            var pictureUrl = await pictureStorageService.StorePictureAsync(
                model.Id,
                request.Identity.Tenant ?? string.Empty,
                request.Base64Image,
                cancellationToken);

            model.AddPicture(pictureUrl);
            await context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success(PaintingProjectsManagement.Features.Models.ModelDetails.FromModel(model));
        }
    }
}
