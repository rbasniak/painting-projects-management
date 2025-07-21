

namespace PaintingProjectsManagement.Features.Models;

public class DeleteModelCategory : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/api/models/categories/{id}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .RequireAuthorization()
        .WithName("Delete Model Category")
        .WithTags("Model Categories");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; }
    }

    public class Validator : SmartValidator<Request, ModelCategory>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
        }
    } 

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var query = _context.Set<ModelCategory>().AsQueryable();
            
            // Filter by tenant if authenticated
            if (request.IsAuthenticated && request.Identity.HasTenant)
            {
                query = query.Where(c => c.TenantId == request.Identity.Tenant);
            }
            
            var category = await query.FirstAsync(x => x.Id == request.Id, cancellationToken);

            _context.Remove(category);
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return CommandResponse.Success();
        }
    }
}