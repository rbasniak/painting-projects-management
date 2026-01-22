namespace PaintingProjectsManagement.Features.Projects;

public class UpdateReferenceColor : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/projects/color-sections/reference-color", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<ColorSectionDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Update Color Section Reference Color")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid SectionId { get; set; }
        public string ReferenceColor { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, ColorSection>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.ReferenceColor)
                .NotEmpty()
                .WithMessage("Reference color is required.")
                .Must(color => IsValidHexColor(color))
                .WithMessage("Reference color must be a valid hex color format (#RRGGBB).");

            RuleFor(x => x.SectionId)
                .MustAsync(async (request, sectionId, cancellationToken) =>
                    await Context.Set<ColorSection>().AnyAsync(
                        s => s.Id == sectionId && 
                        s.ColorGroup.Project.TenantId == request.Identity.Tenant, 
                        cancellationToken))
                .WithMessage("Color section not found.");
        }

        private static bool IsValidHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return false;
            }

            // Check if it's a valid hex color format (#RRGGBB)
            if (color.Length != 7 || !color.StartsWith("#"))
            {
                return false;
            }

            // Check if the remaining 6 characters are valid hex digits
            return color.Substring(1).All(x => (x >= '0' && x <= '9') || (x >= 'A' && x <= 'F') || (x >= 'a' && x <= 'f'));
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var section = await _context.Set<ColorSection>()
                .Include(x => x.ColorGroup)
                    .ThenInclude(x => x.Project)
                .FirstAsync(x => x.Id == request.SectionId &&
                                x.ColorGroup.Project.TenantId == request.Identity.Tenant,
                            cancellationToken);

            section.UpdateReferenceColor(request.ReferenceColor);

            await _context.SaveChangesAsync(cancellationToken);

            // Reload with group for response
            await _context.Entry(section)
                .Reference(x => x.ColorGroup)
                .LoadAsync(cancellationToken);

            var result = ColorSectionDetails.FromModel(section);

            return CommandResponse.Success(result);
        }
    }
}
