

namespace PaintingProjectsManagement.Features.Models;

public class UpdateModelCategory : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/models/categories", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<ModelCategoryDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Update Model Category")
        .WithTags("Model Categories");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, ModelCategory>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            // TODO: detectar unique indexes no SmartValidator
            RuleFor(x => x.Name)
                .MustAsync(async (request, name, cancellationToken) =>
                    !await Context.Set<ModelCategory>().AnyAsync(c => c.Name == name && c.Id != request.Id && c.TenantId == request.Identity.Tenant, cancellationToken))
                .WithMessage("A model category with this name already exists.");
        }
    } 

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var category = await _context.Set<ModelCategory>()
                .Where(c => c.TenantId == request.Identity.Tenant)
                .FirstAsync(x => x.Id == request.Id, cancellationToken);

            category.UpdateDetails(request.Name);
            
            await _context.SaveChangesAsync(cancellationToken);
            
            var result = ModelCategoryDetails.FromModel(category);

            return CommandResponse.Success(result);
        }
    }
}