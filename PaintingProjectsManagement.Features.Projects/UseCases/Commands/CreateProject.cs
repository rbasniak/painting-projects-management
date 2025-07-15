namespace PaintingProjectsManagement.Features.Projects;

internal class CreateProject : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/projects", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok(result);
        })
        .WithName("Create Project")
        .WithTags("Projects");
    }

    public class Request : ICommand<ProjectDetails>
    {
        public string Name { get; set; } = string.Empty;
        public string Base64Image { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .MustAsync(async (name, cancellationToken) => 
                    !await context.Set<Project>().AnyAsync(p => p.Name == name, cancellationToken))
                .WithMessage("A project with this name already exists.");
                
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

    public class Handler(DbContext _context, IFileStorage _fileStorage) : ICommandHandler<Request, ProjectDetails>
    {

        public async Task<CommandResponse<ProjectDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            // Generate a new ID for the project
            var projectId = Guid.NewGuid();
            
            // Store the image and get the URL
            string pictureUrl = await _fileStorage.StoreFileFromBase64Async(
                request.Base64Image,
                $"project_{projectId:N}",
                folderPath: "projects",
                cancellationToken: cancellationToken);
                
            // Create the project
            var project = new Project(
                projectId,
                request.Name,
                pictureUrl,
                DateTime.UtcNow
            );
            
            await _context.AddAsync(project, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success(ProjectDetails.FromModel(project));
        }
    }
}