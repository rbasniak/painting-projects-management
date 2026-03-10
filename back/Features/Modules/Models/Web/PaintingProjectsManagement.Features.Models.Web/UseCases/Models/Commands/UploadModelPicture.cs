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
            RuleFor(x => x.Base64Image)
                .NotEmpty()
                .Must((_, base64Image) => _uploadPolicyService.HasValidImageExtension(base64Image))
                .WithMessage("Invalid image format.");

            RuleFor(x => x.Base64Image)
                .MustAsync(HaveAvailableQuota).WithMessage("Storage quota exceeded.");

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
            var model = await Context.Set<Model>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ModelId, cancellationToken);
            var currentPictures = model?.Pictures.Length ?? 0;

            return await _uploadPolicyService.HasAvailableModelPicturesLimitAsync(
                request.Identity.Tenant ?? string.Empty,
                currentPictures,
                cancellationToken);
        }
    }

    public class Handler(DbContext _context, IModelPictureStorageService pictureStorageService) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var model = await _context.Set<Model>()
                .Include(x => x.Category)
                .FirstAsync(x => x.Id == request.ModelId, cancellationToken);

            string pictureUrl = await pictureStorageService.StorePictureAsync(
                model.Id,
                request.Identity.Tenant ?? string.Empty,
                request.Base64Image,
                cancellationToken);

            model.AddPicture(pictureUrl);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success(ModelDetails.FromModel(model));
        }
    }
}
