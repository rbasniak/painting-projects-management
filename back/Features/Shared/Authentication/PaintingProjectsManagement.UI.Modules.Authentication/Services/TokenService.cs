namespace PaintingProjectsManagement.Blazor.Modules.Authentication;

public interface ITokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);
    Task<string> GetRefreshTokenAsync(CancellationToken cancellationToken);
    Task SetTokensAsync(string accessToken, string refreshToken, CancellationToken cancellationToken);
    Task ClearTokensAsync(CancellationToken cancellationToken);
    Task InitializeTokensFromStorageAsync(CancellationToken cancellationToken);
}

public class TokenService : ITokenService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";

    private readonly IStorageService _storageService;
    private string _accessToken = string.Empty;
    private string _refreshToken = string.Empty;

    public TokenService(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_accessToken);
    }

    public Task<string> GetRefreshTokenAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_refreshToken);
    }

    public async Task InitializeTokensFromStorageAsync(CancellationToken cancellationToken)
    {
        var accessToken = await _storageService.GetItemAsync(AccessTokenKey, StorageType.Local);
        var refreshToken = await _storageService.GetItemAsync(RefreshTokenKey, StorageType.Local);
        
        if (!string.IsNullOrEmpty(accessToken))
            _accessToken = accessToken;
        if (!string.IsNullOrEmpty(refreshToken))
            _refreshToken = refreshToken;
    }

    public async Task SetTokensAsync(string accessToken, string refreshToken, CancellationToken cancellationToken)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        
        // Save tokens to session storage (persists through F5 reloads)
        await _storageService.SetItemAsync(AccessTokenKey, accessToken, StorageType.Local);
        await _storageService.SetItemAsync(RefreshTokenKey, refreshToken, StorageType.Local);
    }

    public async Task ClearTokensAsync(CancellationToken cancellationToken)
    {
        _accessToken = string.Empty;
        _refreshToken = string.Empty;
        
        // Clear tokens from storage
        await _storageService.RemoveItemAsync(AccessTokenKey, StorageType.Local);
        await _storageService.RemoveItemAsync(RefreshTokenKey, StorageType.Local);
    }
} 