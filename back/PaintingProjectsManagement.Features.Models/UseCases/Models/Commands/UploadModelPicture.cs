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
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Base64Image)
                .NotEmpty()
                .WithMessage("Base64 image content is required.")
                .Must(data => IsValidBase64Image(data))
                .WithMessage("Invalid base64 image format. Must be a valid base64 encoded image with proper header.");
        }
        
        private bool IsValidBase64Image(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                return false;
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
            var model = await _context.Set<Model>()
                .Include(m => m.Category)
                .FirstAsync(x => x.Id == request.ModelId, cancellationToken);
                
            if (!string.IsNullOrEmpty(model.PictureUrl))
            {
                await _fileStorage.DeleteFileAsync(model.PictureUrl, cancellationToken);
            }
            
            string pictureUrl = await _fileStorage.StoreFileFromBase64Async(
                request.Base64Image,
                $"model_{model.Id:N}",
                "models",
                2048, 
                2048,
                cancellationToken);
                
            model.UpdatePicture(pictureUrl);
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return CommandResponse.Success();
        }
    }
}