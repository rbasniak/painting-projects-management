using PaintingProjectsManagement.Features.Subscriptions.Integration;

namespace PaintingProjectsManagement.Features.Models;

public sealed class ModelPictureEntitlementProvider(IDispatcher dispatcher) : IModelPictureEntitlementProvider
{
    public async Task<int?> GetMaxModelPicturesPerModelAsync(string tenantId, CancellationToken cancellationToken)
    {
        var entitlementResponse = await dispatcher.SendAsync(
            new GetSubscriptionEntitlementQuery { TenantId = tenantId },
            cancellationToken);

        if (!entitlementResponse.IsValid || entitlementResponse.Data is null)
        {
            return null;
        }

        return entitlementResponse.Data.MaxModelPicturesPerModel;
    }
}
