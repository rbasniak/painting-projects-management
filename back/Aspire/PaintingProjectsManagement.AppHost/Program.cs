using PaintingProjectsManagement.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("ppm-db", 
        userName: builder.AddParameter("PostgresUser", value: "admin"),
        password: builder.AddParameter("PostgresPassword", value: "admin", publishValueAsDefault: true))
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithPgAdmin(pg =>
    {
        pg.WithLifetime(ContainerLifetime.Persistent);
        pg.WithHostPort(5050); 
        pg.WithEnvironment("PGADMIN_DEFAULT_EMAIL", "admin@local.com");
        pg.WithEnvironment("PGADMIN_DEFAULT_PASSWORD", "admin");
    });

var database = postgres.AddDatabase("ppm-postgress");

var pgConnectionString = builder.AddConnectionString(
    "ppm-database",
    ReferenceExpression.Create($"{database};Include Error Detail=true"));

var broker = builder
    .AddRabbitMQ("ppm-rabbitmq", 
        userName: builder.AddParameter("RabbitUser", value: "guest"), 
        password: builder.AddParameter("RabbitPassword", value: "guest", true))
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin(port: 15672)
    .WithDataVolume();

var apiService = builder.AddProject<Projects.PaintingProjectsManagement_Api>("ppm-api")
    .WithReference(pgConnectionString)
    .WithReference(broker)
    .WaitFor(pgConnectionString)
    .WaitFor(broker)
    .WithScalarUI()
    .WithSwaggerUI()
    .WithReDoc()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development");

var blazorApp = builder.AddProject<Projects.PaintingProjectsManagement_UI>("ppm-ui")
    .WithReference(apiService)
    .WaitFor(apiService);

blazorApp.Resource.Annotations.OfType<EndpointAnnotation>().ToList().ForEach(x =>
{
    if (x.Name == "http")
    {
        x.Port = 5251;
        x.IsProxied = false;
    }
    if (x.Name == "https")
    {
        x.Port = 7114;
        x.IsProxied = false;
    }
});

// Note: TUnit test projects don't support Main methods, so we use AddExecutable to run dotnet test via batch script
// Only add in development to avoid including in production builds
var testProjectPath = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "PaintingProjectsManagement.Features.Materials.UI.Tests"));
var testScriptPath = Path.Combine(testProjectPath, "run-tests.cmd");
// Execute the batch file directly - it will handle running dotnet test with proper arguments
var playwrightTests = builder
    .AddExecutable("playwright-tests", testScriptPath, string.Empty, testProjectPath)
    .WithExplicitStart()
    .WithReference(blazorApp)
    .WithEnvironment("ASPIRE", "true")
    .WithEnvironment("EnableTestingPlatformDotnetTestSupport", "true")
    .WithEnvironment("TestingPlatformDotnetTestSupport", "true")
    .ExcludeFromManifest()
    .WithParentRelationship(blazorApp);

builder.Build().Run();
