namespace PaintingProjectsManagement.Features.Paints;

internal class CreatePaintLine : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/paints/lines", async (Request request, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return TypedResults.Ok(result);
        })
        .WithName("Create Brand Line")
        .WithTags("Paint Lines");
    }

    public class Request : ICommand<PaintLineDetails>
    {
        public Guid BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.BrandId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.BrandId).MustAsync(async (brandId, cancellationToken) =>
                await context.Set<PaintBrand>().AnyAsync(b => b.Id == brandId, cancellationToken))
            .WithMessage("Brand does not exist.");
        }
    }

    public class Handler : ICommandHandler<Request, PaintLineDetails>
    {
        private readonly DbContext _context;

        public Handler(DbContext context)
        {
            _context = context;
        }

        public async Task<PaintLineDetails> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var brand = await _context.Set<PaintBrand>()
                .FirstAsync(b => b.Id == request.BrandId, cancellationToken);

            var paintLine = new PaintLine(brand, Guid.NewGuid(), request.Name);
            
            await _context.AddAsync(paintLine, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return PaintLineDetails.FromModel(paintLine);
        }
    }
}