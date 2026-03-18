namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed class CancelSubscription : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/subscriptions/cancel", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<CurrentSubscriptionDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Cancel Subscription")
        .WithTags("Subscriptions");
    }

    public sealed class Request : AuthenticatedRequest, ICommand, ICancelSubscriptionRequest
    {
        public bool CancelAtPeriodEnd { get; set; } = true;
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        private readonly DbContext _context;

        public Validator(DbContext context)
        {
            _context = context;
            RuleFor(x => x)
                .MustAsync(HavePaidSubscriptionAsync)
                .WithMessage("No paid subscription to cancel.");
        }

        private async Task<bool> HavePaidSubscriptionAsync(Request request, CancellationToken cancellationToken)
        {
            var tenant = request.Identity.Tenant ?? string.Empty;
            var current = await _context.Set<TenantSubscription>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenant, cancellationToken);

            return current is not null && current.Tier != SubscriptionTier.Free;
        }
    }

    public sealed class Handler(
        DbContext context,
        ISubscriptionAccessService subscriptionAccessService,
        ISubscriptionTierPolicyCatalog policyCatalog) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var tenant = request.Identity.Tenant ?? string.Empty;
            var subscription = await subscriptionAccessService.GetOrCreateAsync(tenant, cancellationToken);
            var now = DateTime.UtcNow;

            if (request.CancelAtPeriodEnd)
            {
                subscription.MarkCancelAtPeriodEnd();
            }
            else
            {
                subscription.CancelImmediately(now);
            }

            await context.SaveChangesAsync(cancellationToken);

            var entitlement = await subscriptionAccessService.ResolveEntitlementAsync(tenant, cancellationToken);
            return CommandResponse.Success(SubscriptionDetailsMapper.ToCurrentDetails(entitlement, policyCatalog));
        }
    }
}
