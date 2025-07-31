using PaintingProjectsManagement.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.PaintingProjectsManagement_Api>("ppm-api");
    //.WithEnvironment("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true")
    //.WithScalarUI()
    //.WithSwaggerUI()
    //.WithReDoc();

builder.Build().Run();
