using Microsoft.Extensions.Diagnostics.HealthChecks;
using PaintingProjectsManagement.AppHost;
using System.Diagnostics;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.PaintingProjectsManagement_Api>("ppm-api")
    .WithScalarUI()
    .WithSwaggerUI()
    .WithReDoc();

builder.Build().Run();
