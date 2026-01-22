namespace PaintingProjectsManagement.Features.Projects;

public class UpdateColorGroup : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/projects/color-groups", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<ColorGroupDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Update Color Group")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
        public Guid ColorGroupId { get; set; }
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
                        g.Id != request.ColorGroupId &&
                        g.Project.TenantId == request.Identity.Tenant, 
                        cancellationToken))
                .WithMessage("A color group with this name already exists in this project.");

            RuleFor(x => x.ProjectId)
                .MustAsync(async (request, projectId, cancellationToken) =>
                    await Context.Set<Project>().AnyAsync(
                        p => p.Id == projectId && p.TenantId == request.Identity.Tenant, 
                        cancellationToken))
                .WithMessage("Project not found.");

            RuleFor(x => x.ColorGroupId)
                .MustAsync(async (request, colorGroupId, cancellationToken) =>
                    await Context.Set<ColorGroup>().AnyAsync(
                        g => g.Id == colorGroupId && 
                        g.ProjectId == request.ProjectId &&
                        g.Project.TenantId == request.Identity.Tenant, 
                        cancellationToken))
                .WithMessage("Color group not found or does not belong to the specified project.");

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
            var colorGroup = await _context.Set<ColorGroup>()
                .Include(x => x.Sections)
                .FirstAsync(x => x.Id == request.ColorGroupId && 
                                x.ProjectId == request.ProjectId &&
                                x.Project.TenantId == request.Identity.Tenant, 
                            cancellationToken);

            // Validate ProjectId matches (cannot change project)
            if (colorGroup.ProjectId != request.ProjectId)
            {
                throw new InvalidOperationException("ProjectId cannot be changed.");
            }

            // Update name
            colorGroup.UpdateName(request.Name);

            // Get current zones
            var currentZones = colorGroup.Sections.Select(s => s.Zone).ToHashSet();
            var newZones = request.Zones.ToHashSet();

            // Remove sections for zones that are no longer selected
            var zonesToRemove = currentZones.Except(newZones).ToList();
            foreach (var zone in zonesToRemove)
            {
                colorGroup.RemoveSection(zone);
            }

            // Add sections for new zones with default grey
            const string defaultGrey = "#808080";
            var zonesToAdd = newZones.Except(currentZones).ToList();
            foreach (var zone in zonesToAdd)
            {
                colorGroup.AddSection(zone, defaultGrey);
            }

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
