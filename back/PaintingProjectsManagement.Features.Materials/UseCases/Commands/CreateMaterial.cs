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
        .Produces<MaterialDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Create Material")
        .WithTags("Materials");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public string Name { get; set; } = string.Empty;
        public double PackageContentAmount { get; set; }
        public int PackageContentUnit { get; set; }
        public double PackagePriceAmount { get; set; }
        public string PackagePriceCurrency { get; set; } = "USD";
    }

    public class Validator : SmartValidator<Request, Material>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Name)
                .MustAsync(async (request, name, cancellationToken) =>
                {
                    return !await Context.Set<Material>().AnyAsync(x => x.Name == name && x.TenantId == request.Identity.Tenant, cancellationToken);
                })
                .WithMessage(LocalizationService?.LocalizeString(MaterialsMessages.Create.MaterialWithNameAlreadyExists) ?? "A material with this name already exists.");

            RuleFor(x => x.PackageContentAmount)
                .GreaterThan(0)
                .WithMessage("Package amount must be greater than zero.");

            RuleFor(x => x.PackageContentUnit)
                .Must((value) =>
                {
                    // TODO: must be a valid enum value. Create in the library
                    return true;
                })
                .WithMessage("Package amount must be greater than zero.");

            RuleFor(x => x.PackagePriceAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Package price cannot be negative.");

            RuleFor(x => x.PackagePriceCurrency)
                .NotEmpty()
                // TODO: lenght is validades in the model, can we pass that exceptin oto the validation pipeline somehow?
                .Length(3)
                .WithMessage("Currency must be a 3-letter ISO code.")
                .Must(value =>
                {
                    // TODO: Check if valid currency value from external service
                    return true;
                })
                .WithMessage("Invalid currency value.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    { 
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var material = new Material(
                request.Identity.Tenant,
                request.Name,
                new Quantity(request.PackageContentAmount, (PackageContentUnit)request.PackageContentUnit),
                new Money(request.PackagePriceAmount, request.PackagePriceCurrency)
            );

            await _context.AddAsync(material, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var result = MaterialDetails.FromModel(material);

            return CommandResponse.Success(result);
        } 
    }

}
