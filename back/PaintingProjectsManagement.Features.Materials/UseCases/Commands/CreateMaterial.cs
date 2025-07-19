using rbkApiModules.Commons.Core;

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
        .RequireAuthorization()
        .WithName("Create Material")
        .WithTags("Materials");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public string Name { get; set; } = string.Empty;
        public MaterialUnit Unit { get; set; }
        public double PricePerUnit { get; set; }
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
                    return !await Context.Set<Material>().AnyAsync(m => m.Name == name && m.TenantId == request.Identity.Tenant, cancellationToken);
                })
                .WithMessage(LocalizationService?.LocalizeString(MaterialsMessages.Create.MaterialWithNameAlreadyExists) ?? "A material with this name already exists.");

            RuleFor(x => x.PricePerUnit)
                .GreaterThan(0)
                .WithMessage("Price per unit must be greater than zero.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    { 
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var material = new Material(
                request.Identity.Tenant,
                request.Name, 
                request.Unit, 
                request.PricePerUnit
            );
            
            await _context.AddAsync(material, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var result = MaterialDetails.FromModel(material);

            return CommandResponse.Success(result);
        } 
    }

}
