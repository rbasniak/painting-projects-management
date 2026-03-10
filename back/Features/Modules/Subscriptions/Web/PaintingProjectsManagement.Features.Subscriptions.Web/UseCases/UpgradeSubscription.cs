namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed class UpgradeSubscription : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/subscriptions/upgrade", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<CurrentSubscriptionDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Upgrade Subscription")
        .WithTags("Subscriptions");
    }

    public sealed class Request : AuthenticatedRequest, ICommand, IUpgradeSubscriptionRequest
    {
        public SubscriptionTier Tier { get; set; }
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        private readonly DbContext _context;

        public Validator(DbContext context)
        {
            _context = context;

            RuleFor(x => x.Tier)
                .NotEqual(SubscriptionTier.Free)
                .WithMessage("Upgrade target must be a paid tier.");

            RuleFor(x => x)
                .MustAsync(CanUpgradeAsync)
                .WithMessage("Upgrade target must be higher than your current tier.");
        }

        private async Task<bool> CanUpgradeAsync(Request request, CancellationToken cancellationToken)
        {
            var tenant = request.Identity.Tenant ?? string.Empty;
            var current = await _context.Set<TenantSubscription>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenant, cancellationToken);

            var currentTier = current?.Tier ?? SubscriptionTier.Free;
            return request.Tier > currentTier;
        }
    }

    public sealed class Handler(
        DbContext context,
        ISubscriptionAccessService subscriptionAccessService,
        ISubscriptionTierPolicyCatalog policyCatalog,
        IPaymentGateway paymentGateway) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var tenant = request.Identity.Tenant ?? string.Empty;
            var subscription = await subscriptionAccessService.GetOrCreateAsync(tenant, cancellationToken);
            var policy = policyCatalog.Get(request.Tier);
            var now = DateTime.UtcNow;
            var periodEnd = now.AddDays(30);

            var paymentResult = await paymentGateway.ChargeAsync(new PaymentChargeRequest
            {
                TenantId = tenant,
                AmountUsd = policy.MonthlyPriceUsd,
                Description = $"{policy.DisplayName} subscription upgrade"
            }, cancellationToken);

            subscription.ActivatePaidTier(request.Tier, now, periodEnd, paymentResult.TransactionId);

            await context.AddAsync(new SubscriptionPayment(
                tenant,
                subscription.Id,
                request.Tier,
                policy.MonthlyPriceUsd,
                "USD",
                paymentResult.Provider,
                paymentResult.TransactionId,
                SubscriptionPaymentStatus.Succeeded,
                now,
                periodEnd), cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            var entitlement = await subscriptionAccessService.ResolveEntitlementAsync(tenant, cancellationToken);
            return CommandResponse.Success(SubscriptionDetailsMapper.ToCurrentDetails(entitlement, policyCatalog));
        }
    }
}
