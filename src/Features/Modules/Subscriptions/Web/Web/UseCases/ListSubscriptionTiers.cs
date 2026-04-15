namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed class ListSubscriptionTiers : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/subscriptions/tiers", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<IReadOnlyCollection<SubscriptionTierDetails>>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("List Subscription Tiers")
        .WithTags("Subscriptions");
    }

    public sealed class Request : AuthenticatedRequest, IQuery
    {
    }

    public sealed class Validator : AbstractValidator<Request>
    {
    }

    public sealed class Handler(ISubscriptionTierPolicyCatalog policyCatalog)
        : IQueryHandler<Request>
    {
        public Task<QueryResponse> HandleAsync(
            Request request,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<SubscriptionTierDetails> tiers = policyCatalog.List()
                .Select(policy => new SubscriptionTierDetails
                {
                    Tier = policy.Tier,
                    DisplayName = policy.DisplayName,
                    MonthlyPriceUsd = policy.MonthlyPriceUsd,
                    MaxActiveProjects = policy.MaxActiveProjects,
                    MaxInventoryPaints = policy.MaxInventoryPaints,
                    MaxModelPicturesPerModel = policy.MaxModelPicturesPerModel,
                    MaxProjectReferencePicturesPerProject = policy.MaxProjectReferencePicturesPerProject,
                    MaxProjectFinishedPicturesPerProject = policy.MaxProjectFinishedPicturesPerProject,
                    MaxStorageBytes = policy.MaxStorageBytes,
                    AllowHighResolutionImages = policy.AllowHighResolutionImages
                })
                .OrderBy(x => x.Tier)
                .ToArray();

            return Task.FromResult(QueryResponse.Success(tiers));
        }
    }
}
