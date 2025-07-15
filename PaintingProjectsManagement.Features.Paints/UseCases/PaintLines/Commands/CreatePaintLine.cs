namespace PaintingProjectsManagement.Features.Paints;

internal class CreatePaintLine : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/paints/lines", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
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

    public class Handler(DbContext _context) : ICommandHandler<Request, PaintLineDetails>
    {

        public async Task<CommandResponse<PaintLineDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var brand = await _context.Set<PaintBrand>()
                .FirstAsync(b => b.Id == request.BrandId, cancellationToken);

            var paintLine = new PaintLine(brand, Guid.NewGuid(), request.Name);
            
            await _context.AddAsync(paintLine, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success(PaintLineDetails.FromModel(paintLine));
        }
    }
}