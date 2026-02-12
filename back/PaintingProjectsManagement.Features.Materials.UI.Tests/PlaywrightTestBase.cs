using Microsoft.Playwright;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace PaintingProjectsManagement.Features.Materials.UI.Tests;

public class PlaywrightTestBase : IAsyncDisposable
{
    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    protected string BaseUrl { get; private set; } = null!;
    protected string ApiBaseUrl { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Get base URL from environment variable set by Aspire
        // Format: services__ppm-ui__http__0 or services__ppm-ui__https__0
        var httpUrl = Environment.GetEnvironmentVariable("services__ppm-ui__http__0");
        var httpsUrl = Environment.GetEnvironmentVariable("services__ppm-ui__https__0");
        
        BaseUrl = httpsUrl ?? httpUrl ?? "http://localhost:5251";
        
        // Remove trailing slash if present
        BaseUrl = BaseUrl.TrimEnd('/');

        // API base URL - try to get from environment, otherwise use known API ports
        var apiHttpUrl = Environment.GetEnvironmentVariable("services__ppm-api__http__0");
        var apiHttpsUrl = Environment.GetEnvironmentVariable("services__ppm-api__https__0");
        
        // Fallback to known API ports if environment variables aren't set
        if (string.IsNullOrEmpty(apiHttpsUrl) && string.IsNullOrEmpty(apiHttpUrl))
        {
            // Default to HTTPS API URL (matches the hardcoded value in AuthenticationModule)
            apiHttpsUrl = "https://localhost:7236";
        }
        
        // Prefer HTTPS, fallback to HTTP
        ApiBaseUrl = apiHttpsUrl ?? apiHttpUrl ?? "https://localhost:7236";
        ApiBaseUrl = ApiBaseUrl.TrimEnd('/');

        // Initialize Playwright
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        
        // Launch browser (using Chromium for consistency)
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        // Create browser context
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = 1920,
                Height = 1080
            }
        });

        // Create page
        Page = await Context.NewPageAsync();
    }

    public async Task NavigateToAsync(string path)
    {
        var url = path.StartsWith("http") ? path : $"{BaseUrl}{path}";
        await Page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
    }

    public async Task AuthenticateAsync(string username, string password)
    {
        // First, navigate to a page to ensure localStorage is available
        await Page.GotoAsync(BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        // Call the login API endpoint
        // Create HttpClientHandler that bypasses SSL certificate validation for localhost (needed for local development)
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                // Allow localhost certificates in test environment
                if (message.RequestUri?.Host == "localhost" || message.RequestUri?.Host == "127.0.0.1")
                {
                    return true;
                }
                return errors == SslPolicyErrors.None;
            }
        };

        using var httpClient = new HttpClient(handler);
        var loginRequest = new
        {
            Username = username,
            Password = password,
            Tenant = username 
        };

        // Try the primary API URL
        var loginUrl = $"{ApiBaseUrl}/api/authentication/login";
        var response = await httpClient.PostAsJsonAsync(loginUrl, loginRequest);
        
        // If we get a 405 (Method Not Allowed), try the alternate protocol
        if (response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
        {
            // Try the alternate protocol (HTTP if we tried HTTPS, or vice versa)
            var alternateUrl = ApiBaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? loginUrl.Replace("https://localhost:7236", "http://localhost:5191")
                : loginUrl.Replace("http://localhost:5191", "https://localhost:7236");
            
            response?.Dispose();
            response = await httpClient.PostAsJsonAsync(alternateUrl, loginRequest);
            loginUrl = alternateUrl;
        }
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Failed to authenticate. Status: {response.StatusCode}, URL: {loginUrl}, Response: {errorContent}");
        }
        
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        
        if (loginResponse == null || string.IsNullOrEmpty(loginResponse.AccessToken))
        {
            throw new InvalidOperationException("Failed to authenticate: No access token received");
        }

        // Set tokens in browser localStorage
        await Page.EvaluateAsync(@"
            (tokens) => {
                localStorage.setItem('access_token', tokens.accessToken);
                localStorage.setItem('refresh_token', tokens.refreshToken);
            }",
            new { accessToken = loginResponse.AccessToken, refreshToken = loginResponse.RefreshToken });
    }

    private class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public async ValueTask DisposeAsync()
    {
        if (Page != null)
        {
            await Page.CloseAsync();
        }

        if (Context != null)
        {
            await Context.CloseAsync();
        }

        if (Browser != null)
        {
            await Browser.CloseAsync();
        }

        Playwright?.Dispose();
    }
}
