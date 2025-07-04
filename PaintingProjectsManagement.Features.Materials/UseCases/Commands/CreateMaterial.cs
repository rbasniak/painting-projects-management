namespace PaintingProjectsManagement.Features.Materials;

public class CreateMaterial : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/materials", async (Request request, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok(result);
        })
        .WithName("Create Material")
        .WithTags("Materials");
    }

    public class Request : ICommand<MaterialDetails>
    {
        public string Name { get; set; } = string.Empty;
        public MaterialUnit Unit { get; set; }
        public double PricePerUnit { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .MustAsync(async (name, cancellationToken) =>
                    !await context.Set<Material>().AnyAsync(m => m.Name == name, cancellationToken))
                .WithMessage("A material with this name already exists.");

            RuleFor(x => x.PricePerUnit)
                .GreaterThan(0)
                .WithMessage("Price per unit must be greater than zero.");
        }
    }

    public class Handler : ICommandHandler<Request, MaterialDetails>
    {
        private readonly DbContext _context;

        public Handler(DbContext context)
        {
            _context = context;
        }

        public async Task<MaterialDetails> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var material = new Material(request.Name, request.Unit, request.PricePerUnit);
            
            await _context.AddAsync(material, cancellationToken);

            var rtemp = _context.Database.GetConnectionString();

            await _context.SaveChangesAsync(cancellationToken);

            return MaterialDetails.FromModel(material);
        } 
    }

}
