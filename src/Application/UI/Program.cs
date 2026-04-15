using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.UI.Modules.Materials;
using PaintingProjectsManagement.UI.Modules.Models;
using PaintingProjectsManagement.UI.Modules.Subscriptions;
using PaintingProjectsManagement.UI.Modules.Shared;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PaintingProjectsManagement.UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            var apiBaseAddress = ResolveApiBaseAddress(builder);

            builder.Services.AddMudServices();

            builder.Services.AddMaterialsModule(apiBaseAddress);
            builder.Services.AddModelsModule(apiBaseAddress);
            builder.Services.AddProjectsModule(apiBaseAddress);
            builder.Services.AddInventoryModule(apiBaseAddress);
            builder.Services.AddAuthenticationModule(apiBaseAddress);
            builder.Services.AddSubscriptionsModule(apiBaseAddress);

            // Register storage service
            builder.Services.AddScoped<IStorageService, StorageService>();

            builder.Services.AddScoped<ProblemDetailsState>();
            builder.Services.AddTransient<HttpErrorHandler>();

            builder.Services.AddScoped(x => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            var host = builder.Build();

            // Initialize tokens from storage on startup, otherwise it will try to read local storage
            // when JSIntrop it is not available and crash. This way we load the token from memory and 
            // handle it in memory, just updating the localstorage when the token is refreshed
            var tokenService = host.Services.GetRequiredService<ITokenService>();
            await tokenService.InitializeTokensFromStorageAsync(CancellationToken.None);

            await host.RunAsync();
        }

        private static Uri ResolveApiBaseAddress(WebAssemblyHostBuilder builder)
        {
            var configuredApiBaseUrl = builder.Configuration["Api:BaseUrl"];

            if (string.IsNullOrWhiteSpace(configuredApiBaseUrl))
            {
                return new Uri(builder.HostEnvironment.BaseAddress);
            }

            if (Uri.TryCreate(configuredApiBaseUrl, UriKind.Absolute, out var absoluteUri))
            {
                return EnsureTrailingSlash(absoluteUri);
            }

            var relativeUri = new Uri(new Uri(builder.HostEnvironment.BaseAddress), configuredApiBaseUrl);
            return EnsureTrailingSlash(relativeUri);
        }

        private static Uri EnsureTrailingSlash(Uri uri)
        {
            var uriBuilder = new UriBuilder(uri);

            if (!uriBuilder.Path.EndsWith('/'))
            {
                uriBuilder.Path = $"{uriBuilder.Path}/";
            }

            return uriBuilder.Uri;
        }
    }
}
