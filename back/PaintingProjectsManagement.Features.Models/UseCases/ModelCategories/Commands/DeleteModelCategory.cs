

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
        .Produces(StatusCodes.Status200OK)
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
            var category = await _context.Set<ModelCategory>()
                .Where(x => x.TenantId == request.Identity.Tenant)
                .FirstAsync(x => x.Id == request.Id, cancellationToken);

            _context.Remove(category);
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return CommandResponse.Success();
        }
    }
}