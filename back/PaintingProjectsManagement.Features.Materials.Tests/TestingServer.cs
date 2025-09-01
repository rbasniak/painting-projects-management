using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PaintingProjectsManagement.Testing;
using PaintingProjectsManagment.Database;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class TestingServer : RbkTestingServer<Program>
{
    private readonly string _databaseName = $"ppm_test_{Guid.NewGuid():N}";
    private readonly string _connectionString;

    public TestingServer()
    {
        _connectionString = TestingAppHost.CreateDatabaseAsync(_databaseName).GetAwaiter().GetResult();
    }

    protected override bool UseHttps => true;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var values = new Dictionary<string, string?>
            {
                ["ConnectionStrings:ppm-database"] = _connectionString,
                ["ConnectionStrings:ppm-rabbitmq"] = TestingAppHost.RabbitMqConnectionString
            };
            config.AddInMemoryCollection(values);
        });
    }

    public override DbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new DatabaseContext(options);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        TestingAppHost.DropDatabaseAsync(_databaseName).GetAwaiter().GetResult();
    }
}
