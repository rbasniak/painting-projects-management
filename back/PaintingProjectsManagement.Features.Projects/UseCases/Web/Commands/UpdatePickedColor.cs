using System.Text.Json;

namespace PaintingProjectsManagement.Features.Projects;

public class UpdatePickedColor : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/projects/color-sections/picked-color", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<ColorSectionDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Update Picked Color")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid SectionId { get; set; }
        public Guid PaintColorId { get; set; }
    }

    public class Validator : SmartValidator<Request, ColorSection>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.SectionId)
                .MustAsync(async (request, sectionId, cancellationToken) =>
                    await Context.Set<ColorSection>().AnyAsync(
                        s => s.Id == sectionId &&
                        s.ColorGroup.Project.TenantId == request.Identity.Tenant,
                        cancellationToken))
                .WithMessage("Color section not found.");

            RuleFor(x => x.PaintColorId)
                .MustAsync(async (request, paintColorId, cancellationToken) =>
                {
                    var section = await Context.Set<ColorSection>()
                        .Include(s => s.ColorGroup)
                            .ThenInclude(g => g.Project)
                        .FirstAsync(s => s.Id == request.SectionId &&
                                       s.ColorGroup.Project.TenantId == request.Identity.Tenant,
                                 cancellationToken);

                    if (string.IsNullOrWhiteSpace(section.SuggestedColorsJson) || section.SuggestedColorsJson == "[]")
                    {
                        return false;
                    }

                    try
                    {
                        using var doc = JsonDocument.Parse(section.SuggestedColorsJson);
                        var matches = doc.RootElement.EnumerateArray();
                        return matches.Any(m => 
                            m.TryGetProperty("paintColorId", out var idProp) &&
                            Guid.TryParse(idProp.GetString(), out var id) &&
                            id == paintColorId);
                    }
                    catch
                    {
                        return false;
                    }
                })
                .WithMessage("Paint color ID must be one of the suggested colors for this section.");
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

            section.SetPickedColor(request.PaintColorId);

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
