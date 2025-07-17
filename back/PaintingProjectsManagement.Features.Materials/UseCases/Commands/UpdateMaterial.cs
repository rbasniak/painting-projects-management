namespace PaintingProjectsManagement.Features.Materials;

public class UpdateMaterial : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/materials", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .WithName("Update Material")
        .WithTags("Materials");
    }

    public class Request : ICommand
    {
        public Guid Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public MaterialUnit Unit { get; set; }
        public double PricePerUnit { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .MustAsync(async (id, cancellationToken) =>
                    await context.Set<Material>().AnyAsync(m => m.Id == id, cancellationToken))
                .WithMessage("Material with the specified ID does not exist.");
                
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .MustAsync(async (request, name, cancellationToken) => 
                    !await context.Set<Material>().AnyAsync(m => m.Name == name && m.Id != request.Id, cancellationToken))
                .WithMessage("A material with this name already exists.");
                
            RuleFor(x => x.PricePerUnit)
                .GreaterThan(0)
                .WithMessage("Price per unit must be greater than zero.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var material = await _context.Set<Material>().FirstAsync(x => x.Id == request.Id, cancellationToken);

            material.UpdateDetails(request.Name, request.Unit, request.PricePerUnit);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        } 
    }

}
