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
        .RequireAuthorization()
        .WithName("Update Material")
        .WithTags("Materials");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public MaterialUnit Unit { get; set; }
        public double PricePerUnit { get; set; }
    }

    public class Validator : DatabaseConstraintValidator<Request, Material>
    {
        public Validator(DbContext context) : base(context)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Name)
                .MustAsync(async (request, name, cancellationToken) => 
                {
                    return !await Context.Set<Material>().AnyAsync(m => m.Name == name && m.Id != request.Id && m.TenantId == request.Identity.Tenant, cancellationToken);
                })
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
            var query = _context.Set<Material>().AsQueryable();
            
            // Filter by tenant if authenticated
            if (request.IsAuthenticated && request.Identity.HasTenant)
            {
                query = query.Where(m => m.TenantId == request.Identity.Tenant);
            }
            
            var material = await query.FirstAsync(x => x.Id == request.Id, cancellationToken);

            material.UpdateDetails(request.Name, request.Unit, request.PricePerUnit);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        } 
    }

}
