namespace PaintingProjectsManagement.Features.Paints;

internal class CreatePaintBrand : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/paints/brands", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return TypedResults.Ok(result);
        })
        .WithName("Create Paint Brand")
        .WithTags("Paint Brands");
    }

    public class Request : ICommand<PaintBrandDetails>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .MustAsync(async (name, cancellationToken) => 
                    !await context.Set<PaintBrand>().AnyAsync(b => b.Name == name, cancellationToken))
                .WithMessage("A brand with this name already exists.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request, PaintBrandDetails>
    {

        public async Task<CommandResponse<PaintBrandDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var paintBrand = new PaintBrand(Guid.NewGuid(), request.Name);
            
            await _context.AddAsync(paintBrand, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success(PaintBrandDetails.FromModel(paintBrand));
        }
    }
}