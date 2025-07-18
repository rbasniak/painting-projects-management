using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Materials;
using PaintingProjectsManagement.Features.Models;
using PaintingProjectsManagement.Features.Paints;
using PaintingProjectsManagement.Features.Projects;
using rbkApiModules.Commons.Core;
using rbkApiModules.Commons.Core.Helpers;
using PaintingProjectsManagment.Database;
using rbkApiModules.Commons.Relational;
using Scalar.AspNetCore;
using System;

namespace PaintingProjectsManagement.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddMessaging();

        string connectionString;
        if (TestingEnvironmentChecker.IsTestingEnvironment)
        {
            connectionString = $"Data Source=C:\\git\\Development\\Personal\\app_{Guid.NewGuid():N}.db";
        }
        else
        {
            connectionString = "Data Source=C:\\git\\Development\\Personal\\app.db";
        }

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlite(connectionString));
        
        builder.Services.AddScoped<DbContext>(serviceProvider 
            => serviceProvider.GetRequiredService<DatabaseContext>());

        // Register file storage service
        builder.Services.AddSingleton<IFileStorage, LocalFileStorage>();

        var app = builder.Build();

        

        app.MapDefaultEndpoints();

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

        app.UseHttpsRedirection();
        
        // Serve static files from wwwroot
        app.UseStaticFiles();

        app.UseAuthorization();

        app.MapMaterialsFeature();
        app.MapPrintingModelsFeature();
        app.MapPaintsFeature();
        app.MapProjectsFeature();

        app.SetupDatabase<DatabaseContext>(options => options
            .MigrateOnStartup()
        );

        app.Run();
    }
}
