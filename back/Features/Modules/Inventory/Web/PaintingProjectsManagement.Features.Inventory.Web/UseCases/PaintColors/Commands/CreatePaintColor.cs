namespace PaintingProjectsManagement.Features.Inventory;

public class CreatePaintColor : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/paints/colors", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<PaintColorDetails>(StatusCodes.Status200OK)
        .RequireAuthorization(Claims.MANAGE_PAINTS)
        .WithName("Create Paint Color")
        .WithTags("Paint Colors");
    }

    public class Request : ICommand
    {
        public string Name { get; set; } = string.Empty;
        public string HexColor { get; set; } = string.Empty;
        public double BottleSize { get; set; }
        public PaintType Type { get; set; }
        public Guid LineId { get; set; }
        public string? ManufacturerCode { get; set; }
    }

    public class Validator : SmartValidator<Request, PaintColor>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        } 

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.HexColor)
                .Matches("^#[0-9A-Fa-f]{6}$")
                .WithMessage("Hex color must be in format #RRGGBB");

            RuleFor(x => x.BottleSize)
                .GreaterThan(0)
                .WithMessage("Bottle size must be greater than zero.");

            // Check for unique name within the same paint line
            RuleFor(x => x).MustAsync(async (request, cancellationToken) =>
            {
                var exists = await Context.Set<PaintColor>().AnyAsync(x => 
                    x.LineId == request.LineId && 
                    x.Name == request.Name, 
                    cancellationToken);
                return !exists;
            })
            .WithMessage("A paint color with this name already exists in this paint line.");

            // Check for unique hex color within the same paint line
            RuleFor(x => x).MustAsync(async (request, cancellationToken) =>
            {
                var exists = await Context.Set<PaintColor>().AnyAsync(x => 
                    x.LineId == request.LineId && 
                    x.HexColor == request.HexColor, 
                    cancellationToken);
                return !exists;
            })
            .WithMessage("A paint color with this hex color already exists in this paint line.");
        }
    }

    public class Handler(DbContext _context) :  ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var paintLine = await _context.Set<PaintLine>()
                .Include(x => x.Brand)
                .FirstAsync(x => x.Id == request.LineId, cancellationToken);
                
            var paintColor = new PaintColor(
                paintLine,
                request.Name,
                request.HexColor,
                request.BottleSize,
                request.Type,
                request.ManufacturerCode
            );
            
            await _context.AddAsync(paintColor, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            
            return CommandResponse.Success(PaintColorDetails.FromModel(paintColor));
        }
    }
}