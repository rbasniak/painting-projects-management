namespace PaintingProjectsManagement.Features.Paints;

internal class UpdatePaintLine : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/paints/lines", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("Update Brand Line")
        .WithTags("Paint Lines");
    }

    public class Request : ICommand<PaintLineDetails>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid BrandId { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.BrandId).NotEmpty();
            
            RuleFor(x => x.Id).MustAsync(async (id, cancellationToken) =>
                await context.Set<PaintLine>().AnyAsync(x => x.Id == id, cancellationToken))
                .WithMessage("Paint line with the specified ID does not exist.");
            
            RuleFor(x => x.BrandId).MustAsync(async (brandId, cancellationToken) =>
                await context.Set<PaintBrand>().AnyAsync(x => x.Id == brandId, cancellationToken))
                .WithMessage("Brand with the specified ID does not exist.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request, PaintLineDetails>
    {

        public async Task<CommandResponse<PaintLineDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var paintLine = await _context.Set<PaintLine>().FirstAsync(x => x.Id == request.Id, cancellationToken);

            var brand = await _context.Set<PaintBrand>().FirstAsync(x => x.Id == request.BrandId, cancellationToken);
            
            paintLine.UpdateDetails(request.Name, brand);

            await _context.SaveChangesAsync(cancellationToken);
            
            return CommandResponse.Success(PaintLineDetails.FromModel(paintLine));
        }
    }
}