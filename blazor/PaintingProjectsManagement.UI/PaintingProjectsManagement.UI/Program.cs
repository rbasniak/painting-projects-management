using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.UI.Modules.Materials;
using PaintingProjectsManagement.UI.Modules.Models;
using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddMudServices();

            builder.Services.AddMaterialsModule();
            builder.Services.AddModelsModule();
            builder.Services.AddAuthenticationModule();

            // Register storage service
            builder.Services.AddScoped<IStorageService, StorageService>();

            builder.Services.AddScoped<ProblemDetailsState>();
            builder.Services.AddTransient<HttpErrorHandler>();

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            var host = builder.Build();

            // Initialize tokens from storage on startup, otherwise it will try to read local storage
            // when JSIntrop it is not available and crash. This way we load the token from memory and 
            // handle it in memory, just updating the localstorage when the token is refreshed
            var tokenService = host.Services.GetRequiredService<ITokenService>();
            await tokenService.InitializeTokensFromStorageAsync(CancellationToken.None);

            await host.RunAsync();
        }
    }
}
