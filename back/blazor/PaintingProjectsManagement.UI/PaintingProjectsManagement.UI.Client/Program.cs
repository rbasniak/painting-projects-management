using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PaintingProjectsManagement.UI.Client.Modules;
using PaintingProjectsManagement.UI.Client.Modules.Materials.Services;
using PaintingProjectsManagement.UI.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

// Register HttpClient for the application
builder.Services.AddSingleton<HttpClient>();

// Register module services for client-side
builder.Services.AddSingleton<ModuleRegistrationService>();
builder.Services.AddScoped<IMaterialsService, MaterialsService>();

// Register authentication and storage services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddSingleton<TokenService>();

var host = builder.Build();

// Initialize TokenService with token from storage
var tokenService = host.Services.GetRequiredService<TokenService>();
tokenService.InitializeTokenFromStorage();

await host.RunAsync();