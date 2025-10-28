namespace PaintingProjectsManagement.Features.Projects;

public class DeleteProject : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/api/projects/{id}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Delete Project")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
        }
    }

    public class Handler(DbContext _context, IFileStorage _fileStorage) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .Include(x => x.Pictures)
                .Include(x => x.Materials)
                .Include(x => x.References)
                .Include(x => x.ColorGroups)
                .Include(x => x.Steps)
                .FirstAsync(x => x.Id == request.Id, cancellationToken);
                
            if (!string.IsNullOrEmpty(project.PictureUrl))
            {
                await _fileStorage.DeleteFileAsync(project.PictureUrl, cancellationToken);
            }
            
            foreach (var picture in project.Pictures)
            {
                if (!string.IsNullOrEmpty(picture.Url))
                {
                    await _fileStorage.DeleteFileAsync(picture.Url, cancellationToken);
                }
            }
            
            _context.RemoveRange(project.Pictures);
            _context.RemoveRange(project.Materials);
            _context.RemoveRange(project.References);
            _context.RemoveRange(project.ColorGroups);
            _context.RemoveRange(project.Steps);
            _context.Remove(project);
            
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}