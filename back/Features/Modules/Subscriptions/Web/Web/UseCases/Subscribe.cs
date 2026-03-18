namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed class Subscribe : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/subscriptions/subscribe", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<CurrentSubscriptionDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Subscribe")
        .WithTags("Subscriptions");
    }

    public sealed class Request : AuthenticatedRequest, ICommand, ISubscribeRequest
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
                .WithMessage("Please choose a paid tier.");

            RuleFor(x => x)
                .MustAsync(CanSubscribeAsync)
                .WithMessage("Current subscription already active. Use upgrade or cancel first.");
        }

        private async Task<bool> CanSubscribeAsync(Request request, CancellationToken cancellationToken)
        {
            var tenant = request.Identity.Tenant ?? string.Empty;
            var current = await _context.Set<TenantSubscription>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenant, cancellationToken);

            return current is null || current.Tier == SubscriptionTier.Free || current.Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Expired;
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
                Description = $"{policy.DisplayName} subscription"
            }, cancellationToken);

            if (!paymentResult.Succeeded)
            {
                var failedPayment = new SubscriptionPayment(
                    tenant,
                    subscription.Id,
                    request.Tier,
                    policy.MonthlyPriceUsd,
                    "USD",
                    paymentResult.Provider,
                    paymentResult.TransactionId,
                    SubscriptionPaymentStatus.Failed,
                    now,
                    periodEnd,
                    paymentResult.FailureReason);

                await context.AddAsync(failedPayment, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                throw new ExpectedInternalException($"Subscription payment failed: {paymentResult.FailureReason}");
            }

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
