namespace PaintingProjectsManagement.Features.Projects;

public class CreateColorGroup : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/projects/color-groups", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<ColorGroupDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Create Color Group")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public ColorZone[] Zones { get; set; } = Array.Empty<ColorZone>();
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .MustAsync(async (request, name, cancellationToken) =>
                    !await Context.Set<ColorGroup>().AnyAsync(
                        g => g.Name == name && 
                        g.ProjectId == request.ProjectId && 
                        g.Project.TenantId == request.Identity.Tenant, 
                        cancellationToken))
                .WithMessage("A color group with this name already exists in this project.");

            RuleFor(x => x.ProjectId)
                .MustAsync(async (request, projectId, cancellationToken) =>
                    await Context.Set<Project>().AnyAsync(
                        p => p.Id == projectId && p.TenantId == request.Identity.Tenant, 
                        cancellationToken))
                .WithMessage("Project not found.");

            RuleFor(x => x.Zones)
                .Must(zones => zones != null && zones.Length > 0)
                .WithMessage("At least one zone must be selected.");

            RuleFor(x => x.Zones)
                .Must(zones => zones != null && zones.Distinct().Count() == zones.Length)
                .WithMessage("Duplicate zones are not allowed.");

            RuleForEach(x => x.Zones)
                .IsInEnum()
                .WithMessage("Invalid zone value.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var project = await _context.Set<Project>()
                .FirstAsync(x => x.Id == request.ProjectId && x.TenantId == request.Identity.Tenant, cancellationToken);

            var colorGroup = new ColorGroup(project, request.Name);

            // Add to context first so EF Core assigns the Id
            await _context.AddAsync(colorGroup, cancellationToken);
            
            // Add sections for each selected zone with default neutral grey
            // (ColorGroup now has an Id after being added to context)
            const string defaultGrey = "#808080";
            foreach (var zone in request.Zones)
            {
                colorGroup.AddSection(zone, defaultGrey);
            }

            // Add to project
            project.AddColorGroup(colorGroup);

            await _context.SaveChangesAsync(cancellationToken);

            // Reload with sections for response
            await _context.Entry(colorGroup)
                .Collection(x => x.Sections)
                .LoadAsync(cancellationToken);

            var result = ColorGroupDetails.FromModel(colorGroup);

            return CommandResponse.Success(result);
        }
    }
}
