namespace PaintingProjectsManagement.Features.Paints;

internal class UpdatePaintColor : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/paints/colors", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("Update Paint Color")
        .WithTags("Paint Colors");
    }

    public class Request : ICommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HexColor { get; set; } = string.Empty;
        public double BottleSize { get; set; }
        public double Price { get; set; }
        public PaintType Type { get; set; }
        public Guid LineId { get; set; }
        public string? ManufacturerCode { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.HexColor).NotEmpty().MaximumLength(7)
                .Matches("^#[0-9A-Fa-f]{6}$")
                .WithMessage("Hex color must be in format #RRGGBB");
            RuleFor(x => x.BottleSize).GreaterThan(0);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.LineId).NotEmpty();
            
            // Check that the paint color exists
            RuleFor(x => x.Id).MustAsync(async (id, cancellationToken) =>
                await context.Set<PaintColor>().AnyAsync(x => x.Id == id, cancellationToken))
                .WithMessage("Paint color with the specified ID does not exist.");
            
            // Check that the line exists
            RuleFor(x => x.LineId).MustAsync(async (lineId, cancellationToken) =>
                await context.Set<PaintLine>().AnyAsync(x => x.Id == lineId, cancellationToken))
                .WithMessage("Paint line with the specified ID does not exist.");
                
            // Optional manufacturer code validation if provided
            When(x => !string.IsNullOrEmpty(x.ManufacturerCode), () => {
                RuleFor(x => x.ManufacturerCode).MaximumLength(50);
            });
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var paintColor = await _context.Set<PaintColor>()
                .Include(x => x.Line)
                .ThenInclude(x => x.Brand)
                .FirstAsync(x => x.Id == request.Id, cancellationToken);
                
            var line = await _context.Set<PaintLine>()
                .Include(x => x.Brand)
                .FirstAsync(x => x.Id == request.LineId, cancellationToken);
                
            paintColor.UpdateDetails(
                request.Name,
                request.HexColor,
                request.BottleSize,
                request.Type,
                line,
                request.ManufacturerCode
            );
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return CommandResponse.Success(PaintColorDetails.FromModel(paintColor));
        }
    }
}