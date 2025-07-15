namespace PaintingProjectsManagement.Features.Models;

internal class UploadModelPicture : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/models/picture", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok(result);
        })
        .WithName("Upload Model Picture")
        .WithTags("Models");
    }

    public class Request : ICommand<ModelDetails>
    {
        public Guid ModelId { get; set; }
        public string Base64Image { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.ModelId)
                .NotEmpty()
                .MustAsync(async (id, cancellationToken) =>
                    await context.Set<Model>().AnyAsync(m => m.Id == id, cancellationToken))
                .WithMessage("Model with the specified ID does not exist.");
                
            RuleFor(x => x.Base64Image)
                .NotEmpty()
                .WithMessage("Base64 image content is required.")
                .Must(base64 => IsValidBase64Image(base64))
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

    public class Handler(DbContext _context, IFileStorage _fileStorage) : ICommandHandler<Request, ModelDetails>
    {

        public async Task<CommandResponse<ModelDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
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
            
            return CommandResponse.Success(ModelDetails.FromModel(model));
        }
    }
}