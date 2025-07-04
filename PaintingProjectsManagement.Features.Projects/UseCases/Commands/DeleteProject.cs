namespace PaintingProjectsManagement.Features.Projects;

internal class DeleteProject : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/projects/{id}", async (Guid id, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return Results.NoContent();
        })
        .WithName("Delete Project")
        .WithTags("Projects");
    }

    public class Request : ICommand
    {
        public Guid Id { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .MustAsync(async (id, cancellationToken) =>
                    await context.Set<Project>().AnyAsync(p => p.Id == id, cancellationToken))
                .WithMessage("Project with the specified ID does not exist.");
        }
    }

    public class Handler : ICommandHandler<Request>
    {
        private readonly DbContext _context;
        private readonly IFileStorage _fileStorage;

        public Handler(DbContext context, IFileStorage fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .Include(p => p.Pictures)
                .Include(p => p.Materials)
                .Include(p => p.References)
                .Include(p => p.Sections)
                .Include(p => p.Steps)
                .FirstAsync(p => p.Id == request.Id, cancellationToken);
                
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
            _context.RemoveRange(project.Sections);
            _context.RemoveRange(project.Steps);
            _context.Remove(project);
            
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}