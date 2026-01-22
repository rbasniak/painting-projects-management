using PaintingProjectsManagement.Features.Inventory.Integration;

namespace PaintingProjectsManagement.Features.Inventory;

public class FindColorMatches : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        // Note: This endpoint is optional since the command is primarily called via IDispatcher
        // But we can expose it as an HTTP endpoint for direct API access if needed
        endpoints.MapPost("/api/inventory/my-paints/match", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync<Request, IReadOnlyCollection<ColorMatchResult>>(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<IReadOnlyCollection<ColorMatchResult>>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Find Color Matches")
        .WithTags("Inventory");
    }

    public class Request : AuthenticatedRequest, ICommand, IFindColorMatchesCommand
    {
        public string ReferenceColor { get; set; } = string.Empty;
        public int MaxResults { get; set; } = 10;

        // Explicit interface implementation
        string IFindColorMatchesCommand.ReferenceColor => ReferenceColor;
        int IFindColorMatchesCommand.MaxResults => MaxResults;
        string IFindColorMatchesCommand.Tenant => Identity.Tenant ?? string.Empty;
    }

    public class Validator : SmartValidator<Request, UserPaint>
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

            RuleFor(x => x.MaxResults)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("MaxResults must be between 1 and 100.");
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

    public class Handler(DbContext _context) : ICommandHandler<Request, IReadOnlyCollection<ColorMatchResult>>
    {
        public async Task<CommandResponse<IReadOnlyCollection<ColorMatchResult>>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var username = request.Identity.Tenant ?? string.Empty;

            var userPaints = await _context.Set<UserPaint>()
                .Where(up => up.Username == username)
                .Include(up => up.PaintColor)
                .ThenInclude(x => x.Line)
                .ThenInclude(x => x!.Brand)
                .ToListAsync(cancellationToken);

            var matches = userPaints
                .Select(up => new
                {
                    Paint = up,
                    Distance = ColorHelper.CalculateColorDistance(request.ReferenceColor, up.PaintColor.HexColor)
                })
                .OrderBy(x => x.Distance)
                .Take(request.MaxResults)
                .Select(x => new ColorMatchResult
                {
                    PaintColorId = x.Paint.PaintColorId,
                    Name = x.Paint.PaintColor.Name,
                    HexColor = x.Paint.PaintColor.HexColor,
                    BrandName = x.Paint.PaintColor.Line.Brand.Name,
                    LineName = x.Paint.PaintColor.Line.Name,
                    Distance = x.Distance
                })
                .ToList();

            return CommandResponse<IReadOnlyCollection<ColorMatchResult>>.Success(matches);
        }
    }
}
