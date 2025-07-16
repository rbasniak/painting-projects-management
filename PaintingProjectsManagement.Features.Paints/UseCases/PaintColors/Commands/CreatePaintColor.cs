namespace PaintingProjectsManagement.Features.Paints;

internal class CreatePaintColor : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/paints/colors", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("Create Paint Color")
        .WithTags("Paint Colors");
    }

    public class Request : ICommand
    {
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
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.HexColor).NotEmpty().MaximumLength(7)
                .Matches("^#[0-9A-Fa-f]{6}$")
                .WithMessage("Hex color must be in format #RRGGBB");
            RuleFor(x => x.BottleSize).GreaterThan(0);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.LineId).NotEmpty();
            
            RuleFor(x => x.LineId).MustAsync(async (lineId, cancellationToken) =>
                await context.Set<PaintLine>().AnyAsync(x => x.Id == lineId, cancellationToken))
                .WithMessage("Paint line with the specified ID does not exist.");
                
            When(x => !string.IsNullOrEmpty(x.ManufacturerCode), () => {
                RuleFor(x => x.ManufacturerCode).MaximumLength(50);
            });
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
                Guid.NewGuid(),
                paintLine,
                request.Name,
                request.HexColor,
                request.BottleSize,
                request.Price,
                request.Type,
                request.ManufacturerCode
            );
            
            await _context.AddAsync(paintColor, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            
            return CommandResponse.Success(PaintColorDetails.FromModel(paintColor));
        }
    }
}