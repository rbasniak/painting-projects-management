using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Trace;
using PaintingProjectsManagement.Api.Diagnostics;
using PaintingProjectsManagement.Features.Materials;
using PaintingProjectsManagement.Features.Models;
using PaintingProjectsManagement.Features.Paints;
using PaintingProjectsManagement.Features.Projects;
using PaintingProjectsManagment.Database;
using rbkApiModules.Commons.Core;
using rbkApiModules.Commons.Core.UiDefinitions;
using rbkApiModules.Commons.Relational;
using rbkApiModules.Identity.Relational;
using Scalar.AspNetCore;
using System.Reflection;

namespace PaintingProjectsManagement.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddAuthorization();

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracer =>
            {
                tracer
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()

                    // EF Core spans (LINQ queries, SaveChanges, etc.)
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        // Optional: record SQL (beware of PII)
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;

                        // Optional filtering/enrichment
                        // options.Filter = eventData => eventData.Command.CommandText?.Contains("Outbox") != true;
                        options.EnrichWithIDbCommand = (activity, cmd) =>
                        {
                            activity.SetTag("db.name", cmd.Connection.Database);
                            activity.SetTag("db.parameters", cmd.Parameters.Count);
                        };
                    })

                    // ADO.NET provider spans
                    .AddNpgsql() // or .AddSqlClientInstrumentation()

                    // your custom ActivitySource(s)
                    .AddSource("ppm-api")       // whatever you used in EventsTracing.ActivitySource
                    .AddSource("rbk-events");
            });

        var connectionString = builder.Configuration.GetConnectionString("ppm-database");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidDataException($"Could not read Postgres connection string from configuration");
        }

        // TODO: move to the library builder
        builder.Services.AddScoped<IRequestContext, RequestContext>();
        builder.Services.AddScoped<OutboxSaveChangesInterceptor>();

        builder.Services.AddDbContextFactory<MessagingDbContext>(x =>
        {
            x.UseNpgsql(connectionString);
            // normal logging elsewhere
        });

        builder.Services.AddKeyedSingleton<ILoggerFactory>("EfSilent", (serviceProvider, loggerFactory) =>
            LoggerFactory.Create(x => x.ClearProviders()));

        builder.Services.AddKeyedSingleton<IDbContextFactory<MessagingDbContext>>("Silent", (serviceProvider, dbContextFactory) =>
        {
            var silentLogger = serviceProvider.GetRequiredKeyedService<ILoggerFactory>("EfSilent");
            var options = new DbContextOptionsBuilder<MessagingDbContext>()
                .UseNpgsql(connectionString)
                .UseLoggerFactory(silentLogger)
                .EnableSensitiveDataLogging(false)
                .EnableDetailedErrors(false)
                .Options;
            return new PooledDbContextFactory<MessagingDbContext>(options);
        });

        builder.Services.AddDbContext<DatabaseContext>((scope, options) =>
                options.UseNpgsql(connectionString)
                       .EnableSensitiveDataLogging()
                       .AddInterceptors(scope.GetRequiredService<OutboxSaveChangesInterceptor>())
        );

        // Events infrastructure registrations
        builder.Services.AddSingleton<IEventTypeRegistry>(sp => new ReflectionEventTypeRegistry(AppDomain.CurrentDomain.GetAssemblies()));
        builder.Services.Configure<DomainEventDispatcherOptions>(opts =>
        {
            opts.BatchSize = 50;
            opts.PollIntervalMs = 1000;
            opts.MaxAttempts = 10;
            opts.ResolveSilentDbContext = serviceProvider =>
            {
                var contextFactory = serviceProvider.GetRequiredKeyedService<IDbContextFactory<MessagingDbContext>>("Silent");
                return contextFactory.CreateDbContext();    
            };
            opts.ResolveDbContext = serviceProvider =>
            {
                return serviceProvider.GetRequiredService<MessagingDbContext>();
            };
        });

        // TODO: move to the library builder with the possibility to disable it with startup options
        builder.Services.AddHostedService<DomainEventDispatcher>();

        var brokerConnection = builder.Configuration.GetConnectionString("ppm-rabbitmq");

        if (string.IsNullOrEmpty(brokerConnection))
        {
            throw new InvalidDataException($"Could not read RabbitMQ connection string from configuration");
        }

        builder.Services.Configure<BrokerOptions>(opts =>
        {
            var uri = new Uri(brokerConnection);
            opts.HostName = uri.Host;
            opts.Port = uri.Port;
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                var parts = uri.UserInfo.Split(':');
                opts.UserName = parts[0];
                if (parts.Length > 1) opts.Password = parts[1];
            }
            opts.Exchange = "ppm-events";
        });
        builder.Services.AddSingleton<IBrokerPublisher, RabbitMqPublisher>();
        builder.Services.AddSingleton<IBrokerSubscriber, RabbitMqSubscriber>();
        // builder.Services.AddHostedService<IntegrationOutboxRelay>();
        builder.Services.AddSingleton<IIntegrationSubscriberRegistry, IntegrationSubscriberRegistry>();
        builder.Services.AddScoped<IIntegrationOutbox, IntegrationOutbox>();

        // Register domain-to-integration event handlers for Materials and integration consumers for Projects
        
        builder.Services.AddProjectsIntegrationHandlers();

        builder.Services.AddRbkApiCoreSetup(options => options
             .EnableBasicAuthenticationHandler()
             .UseDefaultCompression()
             .UseCustomCors("_defaultPolicy", options =>
                 options.AddPolicy("_defaultPolicy", builder =>
                 {
                     builder
                        .WithOrigins("https://localhost:7233", "https://localhost:7114", "http://localhost:5251", "http://localhost:*")
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

        // Application modules
        builder.Services.AddMaterialsFeature();

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

        // Map outbox health endpoint
        app.MapOutboxHealth();

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

        app.UseMaterialsFeature();
        app.MapPrintingModelsFeature();
        app.MapPaintsFeature();
        app.MapProjectsFeature();

        app.Run();
    }
}
