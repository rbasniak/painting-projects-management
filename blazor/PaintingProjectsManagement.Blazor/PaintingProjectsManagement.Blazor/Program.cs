using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PaintingProjectsManagement.Blazor.Modules.Authentication;
using PaintingProjectsManagement.Blazor.Modules.Materials;

namespace PaintingProjectsManagement.Blazor;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddMaterialsModule();
        builder.Services.AddAuthenticationModule();
        
        // Register storage service
        builder.Services.AddScoped<IStorageService, StorageService>();

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        var host = builder.Build();

        // Initialize tokens from storage on startup
        var tokenService = host.Services.GetRequiredService<ITokenService>();
        await tokenService.InitializeTokensFromStorageAsync(CancellationToken.None);

        await host.RunAsync();
    }
}
