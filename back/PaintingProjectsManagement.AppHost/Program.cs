using PaintingProjectsManagement.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("ppm-db");
var broker = builder.AddRabbitMQ("ppm-rabbit");

var apiService = builder.AddProject<Projects.PaintingProjectsManagement_Api>("ppm-api")
    .WithReference(database)
    .WithReference(broker)
    .WithScalarUI()
    .WithSwaggerUI()
    .WithReDoc()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development");

var blazorApp = builder.AddProject<Projects.PaintingProjectsManagement_UI>("ppm-ui")
    .WithReference(apiService)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development");

builder.Build().Run();
