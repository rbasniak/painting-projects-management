using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials;

public class DeleteMaterial : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/api/materials/{id}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .RequireAuthorization()
        .WithName("Delete Material")
        .WithTags("Materials");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; } 
    }

    public class Validator : DatabaseConstraintValidator<Request, Material>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            // TODO: material cannot be used in any painting project
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

            _context.Remove(material);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        } 
    }

}
