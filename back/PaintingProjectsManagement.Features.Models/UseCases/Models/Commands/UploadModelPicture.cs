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
        public string? FileExtension { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, Model>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Base64Image)
                .NotEmpty()
                .WithMessage("Base64 image content is required.");

            RuleFor(x => x.FileExtension)
                .NotEmpty()
                .WithMessage("Extension must be provided.");
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

            string baseFileName = $"model_{model.Id:N}_{DateTime.UtcNow:yyyyMMddHHmmss}";
            string fullFileName = string.IsNullOrWhiteSpace(request.FileExtension)
                ? baseFileName
                : $"{baseFileName}{request.FileExtension}";

            string pictureUrl = await _fileStorage.StoreFileFromBase64Async(
                request.Base64Image,
                fullFileName,
                "models",
                2048,
                2048,
                cancellationToken);

            model.AddPicture(pictureUrl);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success(ModelDetails.FromModel(model));
        }
    }
}