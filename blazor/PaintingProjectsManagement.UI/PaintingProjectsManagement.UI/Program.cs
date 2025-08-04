using MudBlazor.Services;
using PaintingProjectsManagement.UI.Client.Pages;
using PaintingProjectsManagement.UI.Components;
using PaintingProjectsManagement.UI.Client.Modules.Materials.Services;
using PaintingProjectsManagement.UI.Client.Modules;
using PaintingProjectsManagement.UI.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add HTTP client for API communication
builder.Services.AddHttpClient();

// Register module services
builder.Services.AddScoped<IMaterialsService, MaterialsService>();
builder.Services.AddSingleton<ModuleRegistrationService>();

// Register authentication services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PaintingProjectsManagement.UI.Client._Imports).Assembly);

app.Run();
