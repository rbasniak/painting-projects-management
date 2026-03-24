using System.Net;
using System.Net.Http.Json;

namespace PaintingProjectsManagement.Blazor.Modules.Authentication;

public interface IUserProfileService
{
    Task<ProfileDetailsResponse> GetProfileAsync(CancellationToken cancellationToken);
    Task<CurrentSubscriptionSnapshotResponse> GetCurrentSubscriptionAsync(CancellationToken cancellationToken);
    Task<StorageUsageResponse> GetStorageUsageAsync(CancellationToken cancellationToken);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
}

public sealed class UserProfileService(HttpClient httpClient) : IUserProfileService
{
    public async Task<ProfileDetailsResponse> GetProfileAsync(CancellationToken cancellationToken)
    {
        var profile = await httpClient.GetFromJsonAsync<ProfileDetailsResponse>("profile/me", cancellationToken);
        return profile ?? new ProfileDetailsResponse();
    }

    public async Task<CurrentSubscriptionSnapshotResponse> GetCurrentSubscriptionAsync(CancellationToken cancellationToken)
    {
        var subscription = await httpClient.GetFromJsonAsync<CurrentSubscriptionSnapshotResponse>(
            "subscriptions/me",
            cancellationToken);
        return subscription ?? new CurrentSubscriptionSnapshotResponse();
    }

    public async Task<StorageUsageResponse> GetStorageUsageAsync(CancellationToken cancellationToken)
    {
        var usage = await httpClient.GetFromJsonAsync<StorageUsageResponse>("profile/storage-usage", cancellationToken);
        return usage ?? new StorageUsageResponse();
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        // RBK auth modules expose this route in current backend setup.
        var endpoints = new[]
        {
            "api/authentication/change-password",
            "api/authentication/changePassword"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            if (response.StatusCode != HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        return false;
    }
}
