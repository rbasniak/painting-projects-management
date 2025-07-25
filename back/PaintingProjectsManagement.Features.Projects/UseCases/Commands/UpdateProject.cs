namespace PaintingProjectsManagement.Features.Projects;

internal class UpdateProject : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/projects", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<ProjectHeader>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Update Project")
        .WithTags("Projects");
    }

    public class Request : ICommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Base64Image { get; set; } = string.Empty;
        public DateTime? EndDate { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {

            RuleFor(x => x.EndDate)
                .Must(endDate => !endDate.HasValue || endDate.Value <= DateTime.UtcNow)
                .WithMessage("End date cannot be in the future.");
        } 
    }

    public class Handler(DbContext _context, IFileStorage _fileStorage) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .Include(p => p.Steps)
                .FirstAsync(p => p.Id == request.Id, cancellationToken);

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
                    folderPath: "projects",
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