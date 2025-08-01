using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace PaintingProjectsManagement.UI.Client.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILocalStorageService _localStorage;

    public AuthenticationService(HttpClient httpClient, IConfiguration configuration, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7236";
        _localStorage = localStorage;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/authentication/login", request);
            
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse?.AccessToken != null)
                {
                    await _localStorage.SetItemAsync("authToken", loginResponse.AccessToken);
                    await _localStorage.SetItemAsync("refreshToken", loginResponse.RefreshToken);
                    await _localStorage.SetItemAsync("username", request.Username);
                    await _localStorage.SetItemAsync("tenant", request.Tenant ?? "");
                }
                return loginResponse;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        await _localStorage.RemoveItemAsync("username");
        await _localStorage.RemoveItemAsync("tenant");
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetUsernameAsync()
    {
        return await _localStorage.GetItemAsync<string>("username");
    }

    public async Task<string?> GetTenantAsync()
    {
        return await _localStorage.GetItemAsync<string>("tenant");
    }
}

public interface IAuthenticationService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetUsernameAsync();
    Task<string?> GetTenantAsync();
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Tenant { get; set; }
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public interface ILocalStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
} 