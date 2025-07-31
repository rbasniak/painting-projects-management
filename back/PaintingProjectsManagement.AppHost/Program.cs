using PaintingProjectsManagement.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.PaintingProjectsManagement_Api>("ppm-api")
    .WithScalarUI()
    .WithSwaggerUI()
    .WithReDoc()
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development");

builder.Build().Run();
