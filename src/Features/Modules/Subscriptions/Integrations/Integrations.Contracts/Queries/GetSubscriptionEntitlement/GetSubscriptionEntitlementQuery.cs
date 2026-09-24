namespace PaintingProjectsManagement.Features.Subscriptions.Integration;

public sealed class GetSubscriptionEntitlementQuery : IQuery<SubscriptionEntitlementResult>
{
    public string? TenantId { get; init; }
}
