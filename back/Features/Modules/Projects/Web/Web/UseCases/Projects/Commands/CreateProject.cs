using PaintingProjectsManagement.Features.Subscriptions.Integration;

namespace PaintingProjectsManagement.Features.Projects;

public class CreateProject : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/projects", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<ProjectHeader>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Create Project")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, Project>
    {
        private readonly IDispatcher _dispatcher;

        public Validator(DbContext context, ILocalizationService localization, IDispatcher dispatcher) : base(context, localization)
        {
            _dispatcher = dispatcher;
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .MustAsync(async (request, name, cancellationToken) =>
                    !await Context.Set<Project>().AnyAsync(x => x.Name == name && x.TenantId == request.Identity.Tenant, cancellationToken))
                .WithMessage("A project with this name already exists.");

            RuleFor(x => x)
                .MustAsync(HaveAvailableActiveProjectSlots)
                .WithMessage("Active project limit reached for current subscription tier.");

            // TODO: move to its own use case
            //    RuleFor(x => x.Base64Image)
            //        .NotEmpty()
            //        .WithMessage("Base64 image content is required.")
            //        .Must(base64 => IsValidBase64Image(base64))
            //        .WithMessage("Invalid base64 image format. Must be a valid base64 encoded image with proper header.");
            //}

            //private bool IsValidBase64Image(string base64)
            //{
            //    if (string.IsNullOrEmpty(base64))
            //    {
            //        return false;
            //    }

            //    var hasImagePrefix = base64.StartsWith("data:image/") && base64.Contains(";base64,");
            //    if (!hasImagePrefix)
            //    {
            //        return false;
            //    }

            //    var base64Content = base64.Split(',')[1];

            //    try
            //    {
            //        var bytes = Convert.FromBase64String(base64Content);
            //        return bytes.Length > 0;
            //    }
            //    catch
            //    {
            //        return false;
            //    }
        }

        private async Task<bool> HaveAvailableActiveProjectSlots(Request request, CancellationToken cancellationToken)
        {
            var entitlementResponse = await _dispatcher.SendAsync(
                new GetSubscriptionEntitlementQuery { TenantId = request.Identity.Tenant },
                cancellationToken);
            if (!entitlementResponse.IsValid || entitlementResponse.Data is null)
            {
                return true;
            }

            var maxActiveProjects = entitlementResponse.Data.MaxActiveProjects;
            if (maxActiveProjects == int.MaxValue)
            {
                return true;
            }

            var currentActiveProjects = await Context.Set<Project>()
                .Where(x => x.TenantId == request.Identity.Tenant && x.EndDate == null)
                .CountAsync(cancellationToken);

            return currentActiveProjects < maxActiveProjects;
        }
    }

    public class Handler(DbContext _context, IFileStorage _fileStorage) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            // TODO: remove from the creation and move to its own use case
            //// Store the image and get the URL
            //string pictureUrl = await _fileStorage.StoreFileFromBase64Async(
            //    request.Base64Image,
            //    $"project_{projectId:N}",
            //    folderPath: "projects",
            //    cancellationToken: cancellationToken);

            // Create the project
            var project = new Project(
                request.Identity.Tenant,
                request.Name,
                DateTime.UtcNow,
                null // TODO: add model to the use case
            );

            await _context.AddAsync(project, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var result = ProjectHeader.FromModel(project);

            return CommandResponse.Success(result);
        }
    }
}