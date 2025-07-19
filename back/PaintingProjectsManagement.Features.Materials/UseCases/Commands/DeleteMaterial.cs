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
        .WithName("Delete Material")
        .WithTags("Materials");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; } 
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .MustAsync(async (request, id, cancellationToken) =>
                {
                    return await context.Set<Material>()
                        .Where(m => m.TenantId == request.Identity.Tenant)
                        .AnyAsync(m => m.Id == id, cancellationToken);
                })
                .WithMessage("Material with the specified ID does not exist.");
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
