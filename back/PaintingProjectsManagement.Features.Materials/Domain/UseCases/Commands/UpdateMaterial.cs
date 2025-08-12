namespace PaintingProjectsManagement.Features.Materials;

public class UpdateMaterial : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/materials", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<MaterialDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Update Material")
        .WithTags("Materials");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public double PackageAmount { get; set; }
        public PackageUnits PackageUnit { get; set; }
        public double PackagePriceAmount { get; set; }
        public string PackagePriceCurrency { get; set; } = "USD";
    }

    public class Validator : SmartValidator<Request, Material>
    {
        public Validator(DbContext context) : base(context)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Name)
                .MustAsync(async (request, name, cancellationToken) => 
                {
                    return !await Context.Set<Material>().AnyAsync(x => x.Name == name && x.Id != request.Id && x.TenantId == request.Identity.Tenant, cancellationToken);
                })
                .WithMessage("A material with this name already exists.");

            RuleFor(x => x.PackageAmount)
                .GreaterThan(0)
                .WithMessage("Package amount must be greater than zero.");

            RuleFor(x => x.PackagePriceAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Package price cannot be negative.");

            RuleFor(x => x.PackagePriceCurrency)
                .NotEmpty()
                .Length(3)
                .WithMessage("Currency must be a 3-letter ISO code.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var material = await _context.Set<Material>()
                .Where(x => x.TenantId == request.Identity.Tenant)
                .FirstAsync(x => x.Id == request.Id, cancellationToken);

            material.UpdateDetails(
                request.Name,
                new Quantity(request.PackageAmount, request.PackageUnit),
                new Money(request.PackagePriceAmount, request.PackagePriceCurrency)
            );

            await _context.SaveChangesAsync(cancellationToken);

            var result = MaterialDetails.FromModel(material);  

            return CommandResponse.Success(result);
        } 
    }

}
