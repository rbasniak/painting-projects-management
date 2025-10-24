using PaintingProjectsManagement.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("ppm-db", 
        userName: builder.AddParameter("PostgresUser", value: "admin"),
        password: builder.AddParameter("PostgresPassword", value: "admin", true))
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithPgAdmin(pg =>
    {
        pg.WithLifetime(ContainerLifetime.Persistent);
        pg.WithHostPort(5050); 
        pg.WithEnvironment("PGADMIN_DEFAULT_EMAIL", "admin@local.com");
        pg.WithEnvironment("PGADMIN_DEFAULT_PASSWORD", "admin");
    });

var database = postgres.AddDatabase("ppm-database");

var broker = builder
    .AddRabbitMQ("ppm-rabbitmq", 
        userName: builder.AddParameter("RabbitUser", value: "guest"), 
        password: builder.AddParameter("RabbitPassword", value: "guest", true))
    .WithLifetime(ContainerLifetime.Persistent)
    .WithManagementPlugin(port: 15672)
    .WithDataVolume();

var apiService = builder.AddProject<Projects.PaintingProjectsManagement_Api>("ppm-api")
    .WithReference(database)
    .WithReference(broker)
    .WaitFor(database)
    .WaitFor(broker)
    .WithScalarUI()
    .WithSwaggerUI()
    .WithReDoc()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development");

var blazorApp = builder.AddProject<Projects.PaintingProjectsManagement_UI>("ppm-ui")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
