using PaintingProjectsManagement.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.PaintingProjectsManagement_Api>("ppm-api")
    .WithScalarUI()
    .WithSwaggerUI()
    .WithReDoc();

builder.Build().Run();
