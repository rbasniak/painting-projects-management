namespace PaintingProjectsManagement.Blazor.Modules.Authentication;

using System.Net.Http.Headers;
using System.Net;
using System.Threading;

public class BearerDelegatingHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;
    private readonly IAuthenticationService _authService;
    
    // Static semaphore to prevent concurrent token refreshes across all handler instances
    private static readonly SemaphoreSlim _refreshSemaphore = new SemaphoreSlim(1, 1);

    public BearerDelegatingHandler(ITokenService tokenService, IAuthenticationService authService)
    {
        _tokenService = tokenService;
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await _tokenService.GetAccessTokenAsync(cancellationToken);

        if (!string.IsNullOrEmpty(accessToken))
        {
            // Check if token is expired or expiring soon
            if (JwtTokenHelper.IsTokenExpired(accessToken) || JwtTokenHelper.IsTokenExpiringSoon(accessToken))
            {
                // Acquire semaphore to prevent concurrent token refreshes
                await _refreshSemaphore.WaitAsync(cancellationToken);
                
                try
                {
                    // Double-check token expiration after acquiring lock (in case another request already refreshed it)
                    accessToken = await _tokenService.GetAccessTokenAsync(cancellationToken);
                    
                    if (JwtTokenHelper.IsTokenExpired(accessToken) || JwtTokenHelper.IsTokenExpiringSoon(accessToken))
                    {
                        // Try to refresh the token proactively
                        var refreshToken = await _tokenService.GetRefreshTokenAsync(cancellationToken);
                        
                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            try
                            {
                                var refreshResponse = await _authService.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken }, cancellationToken);

                                if (!string.IsNullOrEmpty(refreshResponse?.AccessToken))
                                {
                                    // Save new tokens
                                    await _tokenService.SetTokensAsync(refreshResponse.AccessToken, refreshResponse.RefreshToken, cancellationToken);
                                    
                                    // Use the new access token
                                    accessToken = refreshResponse.AccessToken;
                                }
                                else
                                {
                                    // Token refresh failed; clear tokens
                                    await _tokenService.ClearTokensAsync(cancellationToken);
                                    accessToken = string.Empty;
                                }
                            }
                            catch
                            {
                                // Token refresh failed; clear tokens
                                await _tokenService.ClearTokensAsync(cancellationToken);
                                accessToken = string.Empty;
                            }
                        }
                        else
                        {
                            // No refresh token available; clear tokens
                            await _tokenService.ClearTokensAsync(cancellationToken);
                            accessToken = string.Empty;
                        }
                    }
                }
                finally
                {
                    // Always release the semaphore
                    _refreshSemaphore.Release();
                }
            }

            // Set the authorization header with the current (or refreshed) token
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
