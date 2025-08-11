using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Materials;
using PaintingProjectsManagement.Features.Models;
using PaintingProjectsManagement.Features.Paints;
using PaintingProjectsManagement.Features.Projects;
using PaintingProjectsManagment.Database;
using rbkApiModules.Commons.Core;
using rbkApiModules.Commons.Core.Helpers;
using rbkApiModules.Commons.Core.UiDefinitions;
using rbkApiModules.Commons.Relational;
using rbkApiModules.Identity.Relational;
using Scalar.AspNetCore;
using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace PaintingProjectsManagement.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddAuthorization();

        string connectionString;
        if (TestingEnvironmentChecker.IsTestingEnvironment)
        {
            var testDbPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "testing", $"testingdb_{Guid.NewGuid():N}.db");
            Directory.CreateDirectory(Path.GetDirectoryName(testDbPath)!);
            connectionString = $"Data Source={testDbPath}";
        }
        else
        {
            connectionString = "Data Source=app.db";
        }

        builder.Services.AddDbContext<DatabaseContext>((scope, options) =>
                options.UseSqlite(connectionString)
                       .EnableSensitiveDataLogging());

        builder.Services.AddRbkApiCoreSetup(options => options
             .EnableBasicAuthenticationHandler()
             .UseDefaultCompression()
             .UseCustomCors("_defaultPolicy", options =>
                 options.AddPolicy("_defaultPolicy", builder =>
                 {
                     builder
                        .WithOrigins("https://localhost:7233")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithExposedHeaders("Content Disposition");
                 }))
             .UseDefaultHsts(builder.Environment.IsDevelopment())
             .UseDefaultHttpsRedirection()
             .UseDefaultMemoryCache()
             .UseDefaultHttpClient()
             // .UseDefaultSwagger("PoC for the new API libraries")
             .UseHttpContextAccessor()
             .UseStaticFiles()
             .RegisterDbContext<DatabaseContext>()
         );

        builder.Services.AddRbkRelationalAuthentication(options => options
            .UseSymetricEncryptationKey()
            .AllowUserCreationByAdmins()
            .AllowUserSelfRegistration()
        );

        builder.Services.AddRbkUIDefinitions(Assembly.GetAssembly(typeof(Program)));

        // Configure OpenAPI with custom schema naming for nested classes
        builder.Services.AddOpenApi(config =>
        {
            //
            // with .NET 9 OpenApi, to support fully qualified type names for nested types in the schema, use the
            // CustomSchemaIds extension method (but from our own extension method :) )
            //
            config.CustomSchemaIds(x => x.FullName?.Split('.').Last().Replace("+", ".", StringComparison.Ordinal));
        });

        // Register file storage service
        builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();

        var app = builder.Build();

        app.UseRbkApiCoreSetup();

        app.UseRbkRelationalAuthentication();

        app.SetupDatabase<DatabaseContext>(options => options
            .MigrateOnStartup()
        );

        app.SetupRbkAuthenticationClaims(options => options
            .WithCustomDescription(x => x.ChangeClaimProtection, "Change claim protection")
            .WithCustomDescription(x => x.ManageClaims, "Manage application claims")
            .WithCustomDescription(x => x.ManageTenantSpecificRoles, "Manage tenant roles")
            .WithCustomDescription(x => x.ManageApplicationWideRoles, "Manage application roles")
            .WithCustomDescription(x => x.ManageTenants, "Manage tenants")
            .WithCustomDescription(x => x.ManageUsers, "Manage users")
            .WithCustomDescription(x => x.ManageUserRoles, "Manage user roles")
            .WithCustomDescription(x => x.OverrideUserClaims, "Override user claims")
        );

        app.SetupRbkDefaultAdmin(options => options
            .WithUsername("superuser")
            .WithPassword("admin")
            .WithDisplayName("Administrator")
            .WithEmail("admin@my-company.com")
        );

        app.UseRbkUIDefinitions();

        app.SeedDatabase<DatabaseSeed>();

        app.MapDefaultEndpoints();

        // app.MapGet("/health", () => Results.Ok("Healthy"));

        // Configure the HTTP request pipeline. 
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Painting Projects Management API v1");
        });
        app.UseReDoc(options =>
        {
            options.SpecUrl("/openapi/v1.json");
        });
        app.MapScalarApiReference();

        app.MapMaterialsFeature();
        app.MapPrintingModelsFeature();
        app.MapPaintsFeature();
        app.MapProjectsFeature();

        app.Run();
    }
}
