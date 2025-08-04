using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace PaintingProjectsManagement.UI.Client.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly TokenService _tokenService;

    public AuthenticationService(HttpClient httpClient, IConfiguration configuration, TokenService tokenService)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7236";
        _tokenService = tokenService;
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
                    _tokenService.SetToken(loginResponse.AccessToken, persist: true);
                }
                return loginResponse;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public void Logout()
    {
        _tokenService.ClearToken();
    }

    public string? GetToken()
    {
        return _tokenService.GetToken();
    }

    public bool IsAuthenticated()
    {
        var token = GetToken();
        return !string.IsNullOrEmpty(token);
    }
}