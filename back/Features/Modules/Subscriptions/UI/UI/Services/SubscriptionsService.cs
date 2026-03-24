using System.Net.Http.Json;
using PaintingProjectsManagement.Features.Subscriptions;

namespace PaintingProjectsManagement.UI.Modules.Subscriptions;

public interface ISubscriptionsService
{
    Task<CurrentSubscriptionResponse> GetCurrentAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SubscriptionTierResponse>> ListTiersAsync(CancellationToken cancellationToken);
    Task<CurrentSubscriptionResponse> SubscribeAsync(SubscriptionTier tier, CancellationToken cancellationToken);
    Task<CurrentSubscriptionResponse> UpgradeAsync(SubscriptionTier tier, CancellationToken cancellationToken);
    Task<CurrentSubscriptionResponse> CancelAsync(bool cancelAtPeriodEnd, CancellationToken cancellationToken);
}

public sealed class SubscriptionsService(HttpClient httpClient) : ISubscriptionsService
{
    public async Task<CurrentSubscriptionResponse> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetFromJsonAsync<CurrentSubscriptionResponse>("subscriptions/me", cancellationToken);
        return response ?? new CurrentSubscriptionResponse();
    }

    public async Task<IReadOnlyCollection<SubscriptionTierResponse>> ListTiersAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetFromJsonAsync<IReadOnlyCollection<SubscriptionTierResponse>>("subscriptions/tiers", cancellationToken);
        return response ?? Array.Empty<SubscriptionTierResponse>();
    }

    public async Task<CurrentSubscriptionResponse> SubscribeAsync(SubscriptionTier tier, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("subscriptions/subscribe", new { Tier = tier }, cancellationToken);
        var data = await response.Content.ReadFromJsonAsync<CurrentSubscriptionResponse>(cancellationToken);
        return data ?? new CurrentSubscriptionResponse();
    }

    public async Task<CurrentSubscriptionResponse> UpgradeAsync(SubscriptionTier tier, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("subscriptions/upgrade", new { Tier = tier }, cancellationToken);
        var data = await response.Content.ReadFromJsonAsync<CurrentSubscriptionResponse>(cancellationToken);
        return data ?? new CurrentSubscriptionResponse();
    }

    public async Task<CurrentSubscriptionResponse> CancelAsync(bool cancelAtPeriodEnd, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("subscriptions/cancel", new { CancelAtPeriodEnd = cancelAtPeriodEnd }, cancellationToken);
        var data = await response.Content.ReadFromJsonAsync<CurrentSubscriptionResponse>(cancellationToken);
        return data ?? new CurrentSubscriptionResponse();
    }
}
