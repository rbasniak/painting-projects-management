
namespace Demo1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<DatabaseContext>((scope, options) => options
                .UseSqlServer(
                    _configuration.GetConnectionString("DefaultConnection").Replace("**CONTEXT**", "Read"))
                //.AddInterceptors(scope.GetRequiredService<DatabaseAnalyticsInterceptor>())
                //.AddInterceptors(scope.GetRequiredService<DatabaseDiagnosticsInterceptor>())
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
            );

            builder.Services.AddRbkApiCoreSetup(options => options
                 .EnableBasicAuthenticationHandler()
                 .UseDefaultCompression()
                 .UseDefaultCors()
                 .UseDefaultHsts(_environment.IsDevelopment())
                 .UseDefaultHttpsRedirection()
                 .UseDefaultMemoryCache()
                 .UseDefaultHttpClient()
                 .UseDefaultSwagger("PoC for the new API libraries")
                 .UseHttpContextAccessor()
                 .UseStaticFiles()
             );     

            builder.Services.AddRbkAuthentication(options => options
                .UseSymetricEncryptationKey()
                .AllowUserCreationByAdmins()
                .AllowUserSelfRegistration()
            );

            builder.Services.AddRbkUIDefinitions(AssembliesForUiDefinitions);

            var app = builder.Build();

            app.UseRbkApiCoreSetup();

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

            app.SeedDatabase<DatabaseSeed>();

            app.Run();
        }
    }
}
