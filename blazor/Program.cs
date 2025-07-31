using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PaintingProjectsManagement.Blazor;
using PaintingProjectsManagement.Blazor.Features.Materials.Services;
using PaintingProjectsManagement.Blazor.Features.Models.Services;
using PaintingProjectsManagement.Blazor.Features.Projects.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register feature services
builder.Services.AddScoped<IMaterialsApiService, MaterialsApiService>();
builder.Services.AddScoped<IModelsApiService, ModelsApiService>();
builder.Services.AddScoped<IModelCategoriesApiService, ModelCategoriesApiService>();
builder.Services.AddScoped<IProjectsApiService, ProjectsApiService>();

builder.Services.AddMudServices();

await builder.Build().RunAsync(); 