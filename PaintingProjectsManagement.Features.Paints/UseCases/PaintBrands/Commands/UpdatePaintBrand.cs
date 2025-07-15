namespace PaintingProjectsManagement.Features.Paints;

internal class UpdatePaintBrand : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/paints/brands", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return TypedResults.Ok(result);
        })
        .WithName("Update Paint Brand")
        .WithTags("Paint Brands");
    }

    public class Request : ICommand<PaintBrandDetails>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            
            RuleFor(x => x.Id).MustAsync(async (id, cancellationToken) =>
                await context.Set<PaintBrand>().AnyAsync(x => x.Id == id, cancellationToken))
                .WithMessage("Paint brand with the specified ID does not exist.");
                
            // Check that the new name is not already taken by another brand
            RuleFor(x => x).MustAsync(async (request, cancellationToken) =>
                !await context.Set<PaintBrand>().AnyAsync(
                    b => b.Name == request.Name && b.Id != request.Id, 
                    cancellationToken))
                .WithMessage("Another brand with this name already exists.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request, PaintBrandDetails>
    {

        public async Task<CommandResponse<PaintBrandDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var paintBrand = await _context.Set<PaintBrand>().FirstAsync(x => x.Id == request.Id, cancellationToken);
            
            paintBrand.UpdateDetails(request.Name);
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return CommandResponse.Success(PaintBrandDetails.FromModel(paintBrand));
        }
    }
}