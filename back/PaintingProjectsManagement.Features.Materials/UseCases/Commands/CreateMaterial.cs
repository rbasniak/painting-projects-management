namespace PaintingProjectsManagement.Features.Materials;

public class CreateMaterial : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/materials", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("Create Material")
        .WithTags("Materials");
    }

    public class Request : ICommand
    {
        public string Name { get; set; } = string.Empty;
        public MaterialUnit Unit { get; set; }
        public double PricePerUnit { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context, ILocalizationService localization)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .MustAsync(async (name, cancellationToken) =>
                    !await context.Set<Material>().AnyAsync(m => m.Name == name, cancellationToken))
                .WithMessage(localization.LocalizeString(MaterialWithNameAlreadyExists.Create.MaterialWithNameAlreadyExists));

            RuleFor(x => x.PricePerUnit)
                .GreaterThan(0)
                .WithMessage("Price per unit must be greater than zero.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    { 

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var material = new Material(request.Name, request.Unit, request.PricePerUnit);
            
            await _context.AddAsync(material, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var result = MaterialDetails.FromModel(material);

            return CommandResponse.Success(result);
        } 
    }

}
