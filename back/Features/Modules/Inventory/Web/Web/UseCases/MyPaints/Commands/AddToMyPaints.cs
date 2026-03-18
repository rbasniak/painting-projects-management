using PaintingProjectsManagement.Features.Inventory.Web;
using PaintingProjectsManagement.Features.Subscriptions.Integration;

namespace PaintingProjectsManagement.Features.Inventory;

public class AddToMyPaints : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/inventory/my-paints", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Add To My Paints")
        .WithTags("Inventory");
    }

    public class Request : AuthenticatedRequest, ICommand, IAddToMyPaintsRequest
    {
        public IReadOnlyList<Guid> PaintColorIds { get; set; } = Array.Empty<Guid>();
    }

    public class Validator : SmartValidator<Request, UserPaint>
    {
        private readonly IDispatcher _dispatcher;

        public Validator(DbContext context, ILocalizationService localization, IDispatcher dispatcher) : base(context, localization)
        {
            _dispatcher = dispatcher;
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x)
                .MustAsync(HaveInventorySlotAvailable)
                .WithMessage("Paint inventory limit reached for current subscription tier.");
        }

        private async Task<bool> HaveInventorySlotAvailable(Request request, CancellationToken cancellationToken)
        {
            var entitlementResponse = await _dispatcher.SendAsync(
                new GetSubscriptionEntitlementQuery { TenantId = request.Identity.Tenant },
                cancellationToken);
            if (!entitlementResponse.IsValid || entitlementResponse.Data is null)
            {
                return true;
            }

            var maxPaints = entitlementResponse.Data.MaxInventoryPaints;

            if (maxPaints == int.MaxValue)
            {
                return true;
            }

            var username = request.Identity.Tenant ?? string.Empty;
            var existingCount = await Context.Set<UserPaint>()
                .Where(x => x.Username == username)
                .Select(x => x.PaintColorId)
                .Distinct()
                .CountAsync(cancellationToken);

            var requestedDistinct = request.PaintColorIds.Distinct().Count();
            var existingRequested = await Context.Set<UserPaint>()
                .Where(x => x.Username == username && request.PaintColorIds.Contains(x.PaintColorId))
                .Select(x => x.PaintColorId)
                .Distinct()
                .CountAsync(cancellationToken);

            var newToAdd = Math.Max(0, requestedDistinct - existingRequested);
            return existingCount + newToAdd <= maxPaints;
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var username = request.Identity.Tenant ?? string.Empty;

            var existing = await _context.Set<UserPaint>()
                .Where(up => up.Username == username && request.PaintColorIds.Contains(up.PaintColorId))
                .Select(up => up.PaintColorId)
                .ToListAsync(cancellationToken);

            var toAdd = request.PaintColorIds
                .Where(id => !existing.Contains(id))
                .Distinct()
                .ToList();

            var existingPaints = await _context.Set<PaintColor>()
                .Where(x => toAdd.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            foreach (var id in toAdd)
            {
                if (!existingPaints.Contains(id))
                    continue;

                _context.Set<UserPaint>().Add(new UserPaint(username, id));
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResponse.Success();
        }
    }
}
