namespace PaintingProjectsManagement.Features.Paints;

internal class DeletePaintBrand : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/paints/brands/{id}", async (Guid id, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return TypedResults.Ok();
        })
        .WithName("Delete Paint Brand")
        .WithTags("Paint Brands");
    }

    public class Request : ICommand
    {
        public Guid Id { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Id).NotEmpty();
            
            // Check that the brand exists
            RuleFor(x => x.Id).MustAsync(async (id, cancellationToken) =>
                await context.Set<PaintBrand>().AnyAsync(x => x.Id == id, cancellationToken))
                .WithMessage("Paint brand with the specified ID does not exist.");
            
            // Check that there are no paint lines associated with this brand
            RuleFor(x => x.Id).MustAsync(async (id, cancellationToken) => 
                !await context.Set<PaintLine>().AnyAsync(x => x.BrandId == id, cancellationToken))
                .WithMessage("Cannot delete a paint brand that has associated paint lines. Remove the paint lines first.");
        }
    }

    public class Handler : ICommandHandler<Request>
    {
        private readonly DbContext _context;

        public Handler(DbContext context)
        {
            _context = context;
        }

        public async Task HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var paintBrand = await _context.Set<PaintBrand>().FirstAsync(x => x.Id == request.Id, cancellationToken);
            
            _context.Remove(paintBrand);
            
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}