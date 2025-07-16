namespace PaintingProjectsManagement.Features.Paints;

internal class CreatePaintLine : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/paints/lines", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("Create Brand Line")
        .WithTags("Paint Lines");
    }

    public class Request : ICommand
    {
        public Guid BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var brand = await _context.Set<PaintBrand>().FirstAsync(b => b.Id == request.BrandId, cancellationToken);
            var line = new PaintLine(brand, Guid.NewGuid(), request.Name);
            
            await _context.AddAsync(line, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}